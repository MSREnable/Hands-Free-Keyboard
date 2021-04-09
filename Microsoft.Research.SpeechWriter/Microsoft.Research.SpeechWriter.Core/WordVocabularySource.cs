using Microsoft.Research.SpeechWriter.Core.Items;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// The word source.
    /// </summary>
    public class WordVocabularySource : PredictiveVocabularySource<SuggestedWordItem>
    {
        private const int SeedSequenceWeight = 1;
        private const int PersistedSequenceWeight = 100;
        private const int LiveSequenceWeight = 10000;

        private readonly StringTokens _tokens;

        private readonly List<int> _vocabularyList = new List<int>();

        private readonly Dictionary<int, int> _tokenToIndex = new Dictionary<int, int>();

        private readonly ObservableCollection<ICommand> _selectedItems = new ObservableCollection<ICommand>();
        private readonly ObservableCollection<ICommand> _runOnSuggestions = new ObservableCollection<ICommand>();

        private readonly OuterSpellingVocabularySource _spellingSource;

        internal WordVocabularySource(ApplicationModel model)
            : base(model, predictorWidth: 4)
        {
            var words = model.Environment.GetOrderedSeedWords();
            _tokens = StringTokens.Create(words);

            _selectedItems.Add(new HeadStartItem(this));
            SelectedItems = new ReadOnlyObservableCollection<ICommand>(_selectedItems);
            RunOnSuggestions = new ReadOnlyObservableCollection<ICommand>(_runOnSuggestions);

            PopulateVocabularyList();

            InitializeUtterance();
            SetRunOnSuggestions();

            var spellingSource = new OuterSpellingVocabularySource(model, this);
            _spellingSource = spellingSource;
        }

        /// <summary>
        /// The maximum number of run on word suggestions to make.
        /// </summary>
        public int MaxRunOnSuggestionsCount { get; } = 8;

        /// <summary>
        /// The currently selected items.
        /// </summary>
        public ReadOnlyObservableCollection<ICommand> SelectedItems { get; }

        /// <summary>
        /// Following words suggestions. (Sequence of several words that may all be used next.)
        /// </summary>
        public ReadOnlyObservableCollection<ICommand> RunOnSuggestions { get; }

        internal IEnumerable<string> Words
        {
            get
            {
                var tokenLimit = _tokens.TokenLimit;
                for (var token = _tokens.TokenStart; token < tokenLimit; token++)
                {
                    var word = _tokens[token];

                    yield return word;
                }
            }
        }

        internal override int[] GetContext() => GetSelectedTokens().ToArray();

        /// <summary>
        /// The number of items within the source.
        /// </summary>
        internal override int Count => _vocabularyList.Count;

        /// <summary>
        /// Load utterances from environment.
        /// </summary>
        /// <returns>Task</returns>
        internal async Task LoadUtterancesAsync()
        {
            var enumerable = Environment.RecallUtterances();
            var enumerator = enumerable.GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    var utterance = enumerator.Current;
                    Debug.Assert(utterance.Length != 0);

                    var sequence = new List<int>(new[] { 0 });
                    foreach (var word in utterance)
                    {
                        Debug.Assert(!string.IsNullOrWhiteSpace(word));
                        var token = _tokens.GetToken(word);
                        Debug.Assert(token != 0);
                        sequence.Add(token);
                    }
                    sequence.Add(0);

                    PersistantPredictor.AddSequence(sequence, PersistedSequenceWeight);
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    await enumerator.DisposeAsync();
                }
            }

            PopulateVocabularyList();
            _model.SetSuggestionsView(this, 0, Count, false);
        }

        internal void ResetUtterances()
        {
            Clear();
            PopulateVocabularyList();
            _model.SetSuggestionsView(this, 0, Count, false);
        }

        /// <summary>
        /// Get the non-zero token for at a given index within the source.
        /// </summary>
        /// <param name="index">Index of item, such that 0 &gt;= index &gt; Count.</param>
        /// <returns>The token at the give index</returns>
        internal override int GetIndexToken(int index)
        {
            return index < 0 ? 0 : _vocabularyList[index];
        }

        internal string GetIndexWord(int index)
        {
            var token = _vocabularyList[index];
            var word = _tokens.GetString(token);
            return word;
        }

        private void PopulateVocabularyList()
        {
            var tokens = GetOrderedVocabularyListTokens();

            _vocabularyList.Clear();
            _tokenToIndex.Clear();

            var index = 0;
            foreach (var token in tokens)
            {
                Debug.Assert(index == _vocabularyList.Count);

                _vocabularyList.Add(token);
                _tokenToIndex.Add(token, index);

                index++;
            }
        }

        internal List<int> GetSelectedTokens()
        {
            var selected = new List<int>(_selectedItems.Count);

            selected.Add(0);
            for (var i = 1; i < _selectedItems.Count; i++)
            {
                var word = _selectedItems[i].ToString();
                var token = _tokens.GetToken(word);
                selected.Add(token);
            }

            return selected;
        }

        internal void InitializeUtterance()
        {
            ClearIncrement();
        }

        private HeadWordItem CreateHeadWordItem(string word)
        {
            var item = new HeadWordItem(this, word);
            return item;
        }

        private GhostWordItem CreateGhostWordItem(string word)
        {
            var item = new GhostWordItem(this, word);
            return item;
        }

        internal SuggestedWordItem CreateSuggestedWordItem(string word)
        {
            var item = new SuggestedWordItem(this, word);
            return item;
        }

        private SuggestedWordItem CreateSuggestedWordSequenceItem(SuggestedWordItem previous, string word)
        {
            var item = new SuggestedWordItem(this, previous, word);
            return item;
        }

        internal SuggestedSpellingWordItem CreateSuggestedSpellingWordItem(string word)
        {
            var item = new SuggestedSpellingWordItem(this, word);
            return item;
        }

        internal void AddSuggestedWord(string word)
        {
            if (_tokens.IsNewWord(word))
            {
                PopulateVocabularyList();
                _spellingSource.AddNewWord(word);
            }

            var item = CreateHeadWordItem(word);
            _selectedItems.Add(item);

            var selection = GetSelectedTokens();
            AddSequence(selection, LiveSequenceWeight);

            SetRunOnSuggestions();
            SetSuggestionsView();
        }

        internal void AddWords(string[] words)
        {
            var newWords = false;

            foreach (var word in words)
            {
                if (_tokens.IsNewWord(word))
                {
                    newWords = true;
                    _spellingSource.AddNewWord(word);
                }

                var item = CreateHeadWordItem(word);
                _selectedItems.Add(item);

                var selection = GetSelectedTokens();
                AddSequence(selection, LiveSequenceWeight);
            }

            if(newWords)
            {
                PopulateVocabularyList();
            }
        }

        internal void AddSuggestedSequence(string[] words)
        {
            AddWords(words);

            SetRunOnSuggestions();
            SetSuggestionsView();
        }

        internal void SetRunOnSuggestions()
        {
            _runOnSuggestions.Clear();

            ContinueRunOnSuggestions();
        }

        internal void TransferRunOnToSuccessors(ICommand runOn)
        {
            bool done;
            do
            {
                var item = (WordItem)_runOnSuggestions[0];
                _runOnSuggestions.RemoveAt(0);

                var selected = CreateHeadWordItem(item.Word);
                _selectedItems.Add(selected);

                var selection = GetSelectedTokens();
                AddSequence(selection, LiveSequenceWeight);

                done = ReferenceEquals(item, runOn);
            }
            while (!done);

            ContinueRunOnSuggestions();
        }

        internal void TakeGhostWord(ICommand runOn)
        {
            TransferRunOnToSuccessors(runOn);
            SetSuggestionsView();
        }

        internal string[] TokensToWords(IEnumerable<int> tokens)
        {
            var words = new List<string>();
            foreach (var token in tokens)
            {
                var word = _tokens.GetString(token);
                words.Add(word);
            }
            return words.ToArray();
        }

        internal void Commit(params string[] words)
        {
            AddWords(words);

            if (_selectedItems.Count == 1 &&
                1 < _runOnSuggestions.Count &&
                _runOnSuggestions[_runOnSuggestions.Count - 1] is GhostStopItem)
            {
                TransferRunOnToSuccessors(_runOnSuggestions[_runOnSuggestions.Count - 2]);
            }

            var selection = GetSelectedTokens();
            selection.Add(0);

            RollbackAndAddSequence(selection, PersistedSequenceWeight);

            while (1 < _selectedItems.Count)
            {
                _selectedItems.RemoveAt(_selectedItems.Count - 1);
            }

            _runOnSuggestions.Clear();
            selection.RemoveAt(0);
            selection.RemoveAt(selection.Count - 1);
            var utterance = new List<string>();
            foreach (var token in selection)
            {
                var word = _tokens.GetString(token);
                utterance.Add(word);
                var item = CreateGhostWordItem(word);
                _runOnSuggestions.Add(item);
            }
            var tail = new GhostStopItem(this, TokensToWords(selection));
            _runOnSuggestions.Add(tail);

            _model.Environment.SaveUtterance(utterance.ToArray());

            InitializeUtterance();
            SetSuggestionsViewComplete();
        }

        internal void TransferSuccessorsToRunOn(ICommand selected)
        {
            var index = _selectedItems.Count - 1;
            var item = _selectedItems[index];
            while (!ReferenceEquals(selected, item))
            {
                var word = item.ToString();
                var runOn = CreateGhostWordItem(word);
                _runOnSuggestions.Insert(0, runOn);
                _selectedItems.RemoveAt(index);

                index--;
                item = _selectedItems[index];
            }

            // TODO: This is very bad, the Stop item should not carry the list, etc., etc., etc.
            if (_runOnSuggestions.Count != 0 && _runOnSuggestions[_runOnSuggestions.Count - 1] is GhostStopItem)
            {
                _runOnSuggestions.RemoveAt(_runOnSuggestions.Count - 1);

                var stopWords = new List<string>();
                foreach (var ghost in _runOnSuggestions)
                {
                    Debug.Assert(ghost is GhostWordItem);
                    stopWords.Add(ghost.ToString());
                }

                var ghostStop = new GhostStopItem(this, stopWords.ToArray());
                _runOnSuggestions.Add(ghostStop);
            }

            SetSuggestionsView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetOrderedVocabularyListTokens()
        {
            var sortedWords = new List<KeyValuePair<string, int>>();
            for (var token = _tokens.TokenStart; token < _tokens.TokenLimit; token++)
            {
                var word = _tokens.GetString(token);
                sortedWords.Add(new KeyValuePair<string, int>(word, token));
            }
            sortedWords.Sort((kv1, kv2) => Environment.Compare(kv1.Key, kv2.Key));

            foreach (var pair in sortedWords)
            {
                Debug.Assert(pair.Value != 0);
                yield return pair.Value;

                // TODO: Add spelling items back
                // yield return -token;
            }
        }

        internal void ContinueRunOnSuggestions()
        {
            var more = true;

            var context = GetSelectedTokens();
            var totalSequence = new List<int>();
            foreach (var item in _runOnSuggestions)
            {
                var wordCommand = item as WordItem;
                if (wordCommand != null)
                {
                    var token = _tokens.GetToken(wordCommand.Word);
                    context.Add(token);
                    totalSequence.Add(token);
                }
                else
                {
                    Debug.Assert(item is GhostStopItem);
                    Debug.Assert(ReferenceEquals(item, _runOnSuggestions[_runOnSuggestions.Count - 1]));

                    more = false;
                }
            }

            if (!more)
            {
                _runOnSuggestions.RemoveAt(_runOnSuggestions.Count - 1);
            }

            while (more && _runOnSuggestions.Count < MaxRunOnSuggestionsCount)
            {
                var token = GetTopToken(context.ToArray());
                if (0 < token)
                {
                    var word = _tokens.GetString(token);
                    var item = CreateGhostWordItem(word);
                    _runOnSuggestions.Add(item);

                    context.Add(token);
                    totalSequence.Add(token);
                }
                else
                {
                    if (token == 0)
                    {
                        var item = new GhostStopItem(this, TokensToWords(totalSequence));
                        _runOnSuggestions.Add(item);
                    }
                    more = false;
                }
            }
        }

        internal override SuggestedWordItem GetIndexItem(int index)
        {
            var token = GetIndexToken(index);
            var word = _tokens.GetString(token);
            var item = CreateSuggestedWordItem(word);
            return item;
        }

        internal override int GetTokenIndex(int token)
        {
            int index;

            if (token == 0)
            {
                index = -1;
            }
            else
            {
                index = _tokenToIndex[token];
            }

            return index;
        }

        internal ISuggestionItem GetNextItem(SuggestedWordItem previousItem, int token)
        {
            ISuggestionItem item;

            if (token != 0)
            {
                var word = _tokens.GetString(token);
                item = CreateSuggestedWordSequenceItem(previousItem, word);
            }
            else
            {
                item = new TailStopItem(this, previousItem.Words);
            }

            return item;
        }

        internal override ICommand CreatePriorInterstitial(int index) => new InterstitialSpellingItem(_spellingSource, index);

        internal override IEnumerable<int> GetTokens()
        {
            var tokenStart = _tokens.TokenStart;
            var tokenLimit = _tokens.TokenLimit;

            var token = tokenLimit;
            while (tokenStart < token)
            {
                token--;
                yield return token;
            }
        }

        internal void ClearIncrement()
        {
            TemporaryPredictor.Clear(); ;
        }

        internal void RollbackAndAddSequence(IReadOnlyList<int> sequence, int increment)
        {
            PersistantPredictor.Subtract(TemporaryPredictor);
            PersistantPredictor.AddSequence(sequence, increment);
        }
    }
}
