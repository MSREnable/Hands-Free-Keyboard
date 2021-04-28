﻿using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// The word source.
    /// </summary>
    public class WordVocabularySource : PredictiveVocabularySource<SuggestedWordItem>
    {
        internal const int SeedSequenceWeight = 1;
        private const int PersistedSequenceWeight = 100;
        private const int LiveSequenceWeight = 10000;

        private readonly StringTokens _tokens;

        private readonly List<int> _vocabularyList = new List<int>();

        private readonly Dictionary<int, int> _tokenToIndex = new Dictionary<int, int>();

        private readonly ObservableCollection<ITile> _headItems = new ObservableCollection<ITile>();
        private readonly ObservableCollection<ITile> _tailItems = new ObservableCollection<ITile>();
        private int _selectedIndex;

        private readonly OuterSpellingVocabularySource _spellingSource;

        private readonly WordTileFilter _tileFilter;

        internal WordVocabularySource(ApplicationModel model)
            : base(model, predictorWidth: 4)
        {
            var words = model.Environment.GetOrderedSeedWords();
            _tokens = StringTokens.Create(words);

            _tileFilter = new WordTileFilter(this, _tokens, CultureInfo.CurrentUICulture);

            _headItems.Add(new HeadStartItem(this));
            _tailItems.Add(null); // Create the slot for the tail item.
            SetSelectedIndex(0);

            HeadItems = new ReadOnlyObservableCollection<ITile>(_headItems);
            TailItems = new ReadOnlyObservableCollection<ITile>(_tailItems);

            PopulateVocabularyList();

            InitializeUtterance();
            SetRunOnSuggestions();

            var spellingSource = new OuterSpellingVocabularySource(model, this);
            _spellingSource = spellingSource;

            ParanoidAssertValid();
        }

        /// <summary>
        /// The maximum number of run on word suggestions to make.
        /// </summary>
        public int MaxRunOnSuggestionsCount { get; } = 8;

        /// <summary>
        /// The currently selected items.
        /// </summary>
        internal ReadOnlyObservableCollection<ITile> HeadItems { get; }

        internal ITile LastTile => _headItems[_selectedIndex];

        /// <summary>
        /// The tail items.
        /// </summary>
        internal ReadOnlyObservableCollection<ITile> TailItems { get; }

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

        /// <summary>
        /// The number of items within the source.
        /// </summary>
        internal override int Count => _vocabularyList.Count;

        internal override ITokenTileFilter TokenFilter => _tileFilter;

        internal override void ResetTileFilter()
        {
            base.ResetTileFilter();

            _tileFilter.Reset();
        }

        private void SetSelectedIndex(int index)
        {
            _selectedIndex = index;

            Debug.Assert(_headItems[0] is HeadStartItem);

            var count = _selectedIndex + 1;
            var context = new List<int>(count) { 0 };
            for (var i = 1; i < count; i++)
            {
                var word = _headItems[i].Content;
                var token = _tokens.GetToken(word);
                context.Add(token);
            }
            SetContext(context);


            Debug.Assert(_tailItems.Count == 1);
            _tailItems[0] = new TailStopItem(_headItems[index], this);
        }

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

        internal void InitializeUtterance()
        {
            ClearIncrement();
        }

        private HeadWordItem CreateHeadWordItem(string word)
        {
            var item = new HeadWordItem(LastTile, this, word);
            return item;
        }

        private GhostWordItem CreateGhostWordItem(ITile predecessor, string word)
        {
            var item = new GhostWordItem(predecessor, this, word);
            return item;
        }

        internal SuggestedWordItem CreateSuggestedWordItem(string word)
        {
            var item = new SuggestedWordItem(LastTile, this, word);
            return item;
        }

        private SuggestedWordItem CreateSuggestedWordSequenceItem(SuggestedWordItem previous, string word)
        {
            var item = new SuggestedWordItem(this, previous, word);
            return item;
        }

        internal SuggestedSpellingWordItem CreateSuggestedSpellingWordItem(string word)
        {
            var item = new SuggestedSpellingWordItem(LastTile, this, word);
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
            _headItems.Insert(_selectedIndex + 1, item);
            SetSelectedIndex(_selectedIndex + 1);

            AddSequence(Context, LiveSequenceWeight);

            SetRunOnSuggestions();
            SetSuggestionsView();

            ParanoidAssertValid();
        }

        internal void AddWords(IEnumerable<string> words)
        {
            var newWords = false;

            ParanoidAssertValid();

            foreach (var word in words)
            {
                if (_tokens.IsNewWord(word))
                {
                    newWords = true;
                    _spellingSource.AddNewWord(word);
                }

                var item = CreateHeadWordItem(word);
                _headItems.Insert(_selectedIndex + 1, item);
                SetSelectedIndex(_selectedIndex + 1);

                AddSequence(Context, LiveSequenceWeight);
            }

            if (newWords)
            {
                PopulateVocabularyList();
            }

            var ghostLimit = _headItems.Count;
            if (_headItems[ghostLimit - 1] is GhostStopItem)
            {
                ghostLimit--;
            }

            var predecessor = _headItems[_selectedIndex];
            for (var i = _selectedIndex + 1; i < ghostLimit; i++)
            {
                var oldGhost = _headItems[i].Content;
                var newGhost = new GhostWordItem(predecessor, this, oldGhost);
                _headItems[i] = newGhost;
                predecessor = newGhost;
            }

            if (ghostLimit != _headItems.Count)
            {
                _headItems[ghostLimit] = new GhostStopItem(predecessor, this);
            }

            ParanoidAssertValid();
        }

        internal void AddSuggestedSequence(string[] words)
        {
            AddWords(words);

            SetRunOnSuggestions();
            SetSuggestionsView();
        }

        internal void SetRunOnSuggestions()
        {
            while (_selectedIndex + 1 < _headItems.Count)
            {
                _headItems.RemoveAt(_headItems.Count - 1);
            }

            ContinueRunOnSuggestions();

            ParanoidAssertValid();
        }

        internal void TransferRunOnToSuccessors(ITile runOn)
        {
            ParanoidAssertValid();

            bool done;
            do
            {
                var item = (WordItem)_headItems[_selectedIndex + 1];

                var selected = CreateHeadWordItem(item.Content);
                _headItems[_selectedIndex + 1] = selected;
                SetSelectedIndex(_selectedIndex + 1);

                AddSequence(Context, LiveSequenceWeight);

                done = ReferenceEquals(item, runOn);
            }
            while (!done);

            ContinueRunOnSuggestions();

            ParanoidAssertValid();
        }

        internal void TakeGhostWord(ITile runOn)
        {
            TransferRunOnToSuccessors(runOn);
            SetSuggestionsView();
        }

        internal void Commit(ITile stopTile)
        {
            ParanoidAssertValid();

            var wordList = new List<string>();
            var position = stopTile.Predecessor;
            while (!ReferenceEquals(LastTile, position))
            {
                wordList.Insert(0, position.Content);
                position = position.Predecessor;
            }

            AddWords(wordList);

            if (_selectedIndex == 0 &&
                2 < _headItems.Count &&
                _headItems[_headItems.Count - 1] is GhostStopItem)
            {
                TransferRunOnToSuccessors(_headItems[_headItems.Count - 2]);
            }

            ParanoidAssertValid();

            var selection = new List<int>(Context) { 0 };

            RollbackAndAddSequence(selection, PersistedSequenceWeight);

            var predecessor = _headItems[0];
            for (var i = 1; i <= _selectedIndex; i++)
            {
                var selected = _headItems[i];
                var ghost = CreateGhostWordItem(predecessor, selected.Content);
                _headItems[i] = ghost;
                predecessor = ghost;
            }

            while (_selectedIndex + 1 < _headItems.Count)
            {
                _headItems.RemoveAt(_headItems.Count - 1);
            }

            selection.RemoveAt(0);
            selection.RemoveAt(selection.Count - 1);
            var utterance = new List<string>();
            foreach (var token in selection)
            {
                var word = _tokens.GetString(token);
                utterance.Add(word);
            }

            SetSelectedIndex(0);
            var tail = new GhostStopItem(predecessor, this);
            _headItems.Add(tail);

            _model.Environment.SaveUtterance(utterance.ToArray());

            InitializeUtterance();
            SetSuggestionsViewComplete();

            ParanoidAssertValid();
        }

        internal void TransferSuccessorsToRunOn(ITile selected)
        {
            // Find the selected tile.
            var index = 0;
            while (!ReferenceEquals(selected, _headItems[index]))
            {
                index++;
            }
            Debug.Assert(index <= _selectedIndex);

            var predecessor = _headItems[index];

            var ghostIndex = index + 1;
            var ghostLimit = _headItems.Count;
            if (_headItems[ghostLimit - 1] is GhostStopItem)
            {
                ghostLimit--;
            }
            for (; ghostIndex < ghostLimit; ghostIndex++)
            {
                var oldItem = _headItems[ghostIndex];
                var encoding = oldItem.Content;
                var newItem = new GhostWordItem(predecessor, this, encoding);
                _headItems[ghostIndex] = newItem;
                predecessor = newItem;
            }

            SetSelectedIndex(index);

            if (ghostLimit < _headItems.Count)
            {
                Debug.Assert(ghostLimit + 1 == _headItems.Count);

                var ghostStop = new GhostStopItem(predecessor, this);
                _headItems[ghostLimit] = ghostStop;
            }

            SetSuggestionsView();

            ParanoidAssertValid();
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
                if (word[0] != '\0')
                {
                    sortedWords.Add(new KeyValuePair<string, int>(word, token));
                }
            }
            sortedWords.Sort((kv1, kv2) => Environment.Compare(kv1.Key, kv2.Key));

            var commandIndex = 0;
            foreach (var enumName in Enum.GetNames(typeof(TileCommand)))
            {
                var command = '\0' + enumName;
                var token = _tokens.GetToken(command);
                sortedWords.Insert(commandIndex, new KeyValuePair<string, int>(command, token));
            }

            foreach (var pair in sortedWords)
            {
                Debug.Assert(pair.Value != 0);
                yield return pair.Value;
            }
        }

        internal void ContinueRunOnSuggestions()
        {
            var context = new List<int>
            {
                0
            };
            ITile predecessor = _headItems[0];
            for (var i = 1; i <= _selectedIndex; i++)
            {
                var word = (WordItem)_headItems[i];
                Debug.Assert(predecessor == word.Predecessor);
                predecessor = word;
                var token = _tokens.GetToken(word.Content);
                context.Add(token);
            }

            var count = _headItems.Count;
            if (_headItems[count - 1] is GhostStopItem)
            {
                count--;
                _headItems.RemoveAt(count);
            }


            for (var i = _selectedIndex + 1; i < count; i++)
            {
                var word = (GhostWordItem)_headItems[i];
                var token = _tokens.GetToken(word.Content);
                context.Add(token);

                word = new GhostWordItem(predecessor, this, word.Content);
                _headItems[i] = word;
                predecessor = word;
            }

            var more = true;
            while (more && _headItems.Count - _selectedIndex - 1 < MaxRunOnSuggestionsCount)
            {
                var token = GetTopToken(context.ToArray());
                if (0 < token)
                {
                    var word = _tokens.GetString(token);
                    var item = CreateGhostWordItem(predecessor, word);
                    _headItems.Add(item);

                    context.Add(token);

                    predecessor = item;
                }
                else
                {
                    if (token == 0)
                    {
                        var item = new GhostStopItem(predecessor, this);
                        _headItems.Add(item);
                    }
                    more = false;
                }
            }

            {
                // TODO: Old code seemed to just remove terminal GhostStopItem if it was present! Was that necessary?
                // was _runOnSuggestions.RemoveAt(_runOnSuggestions.Count - 1);
                // could be: _headItems.RemoveAt(headItems.Count - 1);
            }

            ParanoidAssertValid();
        }

        internal override SuggestedWordItem GetIndexItem(int index)
        {
            var token = GetIndexToken(index);
            var word = _tokens.GetString(token);
            var item = CreateSuggestedWordItem(word);
            return item;
        }

        internal override IEnumerable<ITile> CreateSuggestionList(int index)
        {
            IEnumerable<ITile> value;

            var token = GetIndexToken(index);
            var word = _tokens.GetString(token);
            if (word[0] != '\0')
            {
                value = base.CreateSuggestionList(index);
            }
            else
            {
                var command = (TileCommand)Enum.Parse(typeof(TileCommand), word.Substring(1));
                var tile = new CommandItem(null, this, command);
                value = new[] { tile };
            }

            return value;
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
                item = new TailStopItem(previousItem, this);
            }

            return item;
        }

        internal override ITile CreatePriorInterstitial(int index)
        {
            ITile value;

            var token = GetIndexToken(index);
            var word = _tokens.GetString(token);
            if (word[0] != '\0')
            {
                value = new InterstitialSpellingItem(LastTile, _spellingSource, index);
            }
            else
            {
                value = null;
            }

            return value;
        }

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

        [Conditional("DEBUG")]
        internal void AssertValid()
        {
            bool checkPredecessor = true;

            Debug.Assert(1 <= _headItems.Count, "At least one item in the head");
            Debug.Assert(_headItems[0] is HeadStartItem, "First item is the start item");
            Debug.Assert(0 <= _selectedIndex, "Selected item is first or subsequent item");
            Debug.Assert(_selectedIndex < _headItems.Count);

            ITile predecessor = _headItems[0];
            for (var i = 1; i <= _selectedIndex; i++)
            {
                Debug.Assert(_headItems[i] is HeadWordItem, "Items 1.._selectedIndex inclusive are Head Words");
                Debug.Assert(!checkPredecessor || ReferenceEquals(predecessor, _headItems[i].Predecessor), "Correctly linked Head Word predecessor");
                predecessor = _headItems[i];
            }

            var ghostWordLimit = _headItems.Count;
            if (_headItems[ghostWordLimit - 1] is GhostStopItem)
            {
                ghostWordLimit--;
            }

            for (var i = _selectedIndex + 1; i < ghostWordLimit; i++)
            {
                Debug.Assert(_headItems[i] is GhostWordItem, "Items _selectedIndex+1..ghostWordLimit inclusive are Ghost Words");
                Debug.Assert(!checkPredecessor || ReferenceEquals(predecessor, _headItems[i].Predecessor), "Correctly linked Ghost Word predecessor");
                predecessor = _headItems[i];
            }

            if (ghostWordLimit != _headItems.Count)
            {
                Debug.Assert(_headItems[ghostWordLimit] is GhostStopItem, "Item ghostWordLimit is a Ghost Stop");
                Debug.Assert(!checkPredecessor || ReferenceEquals(predecessor, _headItems[ghostWordLimit].Predecessor), "Correctly linked Ghost Word predecessor");
                predecessor = _headItems[ghostWordLimit];
            }

            Debug.Assert(!checkPredecessor || ReferenceEquals(predecessor, _headItems[_headItems.Count - 1]), "Fully linked _headItems");
        }

        [Conditional("PARANOID")]
        internal void ParanoidAssertValid()
        {
            AssertValid();
        }
    }
}
