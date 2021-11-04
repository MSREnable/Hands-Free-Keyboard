﻿using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
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

        internal void ExecuteCommand(TileCommand command)
        {
            switch (command)
            {
                default:
                    throw new NotImplementedException();

                case TileCommand.Typing:
                    {
                        var source = new CaseWordVocabularySource(Model, this, (HeadWordItem)LastTile);
                        source.SetSuggestionsView();
                    }
                    break;

                case TileCommand.Code:
                    {
                        var source = new CodeVocabularySource(this);
                        source.SetSuggestionsView();
                    }
                    break;
            }
        }

        private int _selectedIndex;

        private readonly SpellingVocabularySource _spellingSource;

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

            var spellingSource = new SpellingVocabularySource(model, this);
            _spellingSource = spellingSource;

            ParanoidAssertValid();
        }

        internal int SelectedIndex => _selectedIndex;

        internal SpellingVocabularySource SpellingSource => _spellingSource;

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

        internal bool IsCommandVisible(TileCommand command)
        {
            bool value;

            switch (command)
            {
                case TileCommand.Typing:
                    value = LastTile is HeadWordItem;
                    break;

                case TileCommand.Code:
                    value = true;
                    break;

                default:
                    value = false;
                    break;
            }

            return value;
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
            using (var reader = await Environment.RecallUtterancesAsync())
            {
                for (var line = await reader.ReadLineAsync();
                    line != null;
                    line = await reader.ReadLineAsync())
                {
                    var utteranceData = UtteranceData.FromLine(line);
                    var utterance = utteranceData.Sequence;

                    Debug.Assert(utterance.Count != 0);

                    var sequence = new List<int>(new[] { 0 });
                    foreach (var tile in utterance)
                    {
                        var tokenString = tile.ToTokenString();
                        Debug.Assert(!string.IsNullOrWhiteSpace(tokenString));
                        var token = _tokens.GetToken(tokenString);
                        Debug.Assert(token != 0);
                        sequence.Add(token);
                    }
                    sequence.Add(0);

                    PersistantPredictor.AddSequence(sequence, PersistedSequenceWeight);
                }
            }

            PopulateVocabularyList();
            SetSuggestionsView(0, Count, false);
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

            AddSequenceTail(Context, PersistedSequenceWeight);

            SetRunOnSuggestions();
            SetSuggestionsView();

            ParanoidAssertValid();
        }

        internal void ReplaceLastItem(string word)
        {
            while (_selectedIndex < _headItems.Count)
            {
                _headItems.RemoveAt(_headItems.Count - 1);
            }
            _selectedIndex--;

            AddSuggestedWord(word);
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

                AddSequenceTail(Context, PersistedSequenceWeight);
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

                AddSequenceTail(Context, PersistedSequenceWeight);

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

            // Find position from which to work backwards to rebirth ghost items.
            ITile position;
            if (_selectedIndex == 0 &&
                !ReferenceEquals(stopTile, _headItems[_headItems.Count - 1]) &&
                _headItems[_headItems.Count - 1] is GhostStopItem)
            {
                position = _headItems[_headItems.Count - 1].Predecessor;
            }
            else
            {
                position = stopTile.Predecessor;
            }

            var wordList = new List<string>();
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
            var tiles = new List<TileData>();
            foreach (var token in selection)
            {
                var tokenString = _tokens.GetString(token);
                var tile = TileData.FromTokenString(tokenString);
                tiles.Add(tile);
            }
            var utterance = TileSequence.FromData(tiles);

            SetSelectedIndex(0);
            var tail = new GhostStopItem(predecessor, this);
            _headItems.Add(tail);

            Model.SaveUtterance(utterance);

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

            if (index != _selectedIndex)
            {
                var subContext = new List<int>(Context);
                subContext.RemoveRange(_selectedIndex + 1, subContext.Count - _selectedIndex - 1);
                for (var i = _selectedIndex; index < i; i--)
                {
                    AddSequenceTail(subContext, LiveSequenceWeight);
                    subContext.RemoveAt(i);
                }
            }

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

            var commandNames = Enum.GetNames(typeof(TileCommand));
            for (var i = 0; i < commandNames.Length; i++)
            {
                var command = '\0' + commandNames[i];
                var token = _tokens.GetToken(command);
                sortedWords.Insert(i, new KeyValuePair<string, int>(command, token));
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

        internal override IReadOnlyList<ITile> CreateSuggestionList(int index)
        {
            IReadOnlyList<ITile> value;

            var token = GetIndexToken(index);
            var word = _tokens.GetString(token);
            if (word[0] != '\0')
            {
                value = base.CreateSuggestionList(index);
            }
            else
            {
                var command = (TileCommand)Enum.Parse(typeof(TileCommand), word.Substring(1));
                var tile = new CommandItem(LastTile, this, command);
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

        private WordPrediction GetNextCorePrediction(IEnumerator<int[]> enumerator)
        {
            WordPrediction prediction;

            if (enumerator.MoveNext())
            {
                var score = enumerator.Current;
                var token = score[0];
                var index = GetTokenIndex(token);
                var text = _tokens[token];

                prediction = new WordPrediction(score, index, text);
            }
            else
            {
                prediction = null;
            }

            return prediction;
        }

        private WordPrediction GetTopPrediction(ScoredTokenPredictionMaker maker)
        {
            WordPrediction value;

            var topScores = maker.GetTopScores(0, Count, true);
            using (var enumerator = topScores.GetEnumerator())
            {
                value = GetNextCorePrediction(enumerator);
            }

            if (value.Score.Length < 3)
            {
                // TODO: We should not have constructed this object.
                value = null;
            }
            else
            {
                Debug.WriteLine($"Health predicition {value}");
            }

            return value;
        }

        private void DisplayInitialCorePredictions(List<List<WordPrediction>> coreCompoundPredicitions, List<WordPrediction> followOnPredictions, WordPrediction wordPrediction)
        {
            Debug.WriteLine("Initial core position:");
            Debug.Indent();

            for (var i = 0; i < coreCompoundPredicitions.Count; i++)
            {
                var builder = new StringBuilder();

                foreach (var group in coreCompoundPredicitions[i])
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(" / ");
                    }

                    var groupText = string.Join(" ", group);

                    builder.Append(groupText);
                }
                Debug.WriteLine($"{builder} {followOnPredictions[i]}");
            }
            Debug.WriteLine($"--- {wordPrediction}");

            Debug.Unindent();
        }

        private static int CompareScores(int[] lhs, int[] rhs)
        {
            var comparison = lhs.Length.CompareTo(rhs.Length);

            for (var i = lhs.Length - 1; comparison == 0 && 0 <= i; i--)
            {
                comparison = lhs[i].CompareTo(rhs[i]);
            }

            return comparison;
        }

        protected override SortedList<int, IReadOnlyList<ITile>> CreateSuggestionLists(int lowerBound, int upperBound, int maxListCount)
        {
            var maxListItemCount = Math.Max(1, Model.DisplayColumns / 2);

            var maker = PersistantPredictor.CreatePredictionMaker(this, TokenFilter, Context);
            var scores = maker.GetTopScores(lowerBound, upperBound, TokenFilter == null);

            var corePredicitions = new List<WordPrediction>(maxListCount);
            var coreCompoundPredictions = new List<List<WordPrediction>>(maxListCount);

            var followOnPredictions = new List<WordPrediction>(maxListCount);

            var predictedTokens = new HashSet<int>();

            using (var enumerator = scores.GetEnumerator())
            {
                // Seed predictions with most likely items.
                var nextCorePrediction = GetNextCorePrediction(enumerator);
                for (var iteration = 0; iteration < maxListCount && nextCorePrediction != null; iteration++)
                {
                    AddNextPrediction(nextCorePrediction);
                    nextCorePrediction = GetNextCorePrediction(enumerator);
                }

                // Apply strategies to improve the core predictions.
                for (var improved = true; improved;)
                {
                    improved = false;

                    Debug.Assert(coreCompoundPredictions.Count == corePredicitions.Count);
                    Debug.Assert(followOnPredictions.Count == corePredicitions.Count);

                    if (1 < corePredicitions.Count && nextCorePrediction != null)
                    {
                        var pairable = -1;
                        var lostPrediction = (WordPrediction)null;

                        var next = corePredicitions[0];
                        var definiteImprovementFound = false;
                        for (var i = 1; !definiteImprovementFound && i < corePredicitions.Count; i++)
                        {
                            var prev = next;
                            next = corePredicitions[i];

                            if (next.Text.StartsWith(prev.Text))
                            {
                                var prevCount = coreCompoundPredictions[i - 1].Count;
                                var prevTailPrediction = coreCompoundPredictions[i - 1][prevCount - 1];
                                var prevTailText = prevTailPrediction.Text;

                                var nextCount = coreCompoundPredictions[i].Count;
                                var nextTailPrediction = coreCompoundPredictions[i][nextCount - 1];
                                var nextTailText = nextTailPrediction.Text;

                                bool canMerge;

                                WordPrediction potentialLostPrediction;
                                if (nextTailText.StartsWith(prevTailText))
                                {
                                    canMerge = true;
                                    potentialLostPrediction = followOnPredictions[i - 1];
                                }
                                else if (prevTailText.StartsWith(nextTailText))
                                {
                                    canMerge = true;
                                    potentialLostPrediction = followOnPredictions[i];
                                }
                                else
                                {
                                    canMerge = false;
                                    potentialLostPrediction = null;
                                }

                                if (canMerge)
                                {
                                    if (pairable == -1 || potentialLostPrediction == null)
                                    {
                                        pairable = i - 1;
                                        lostPrediction = potentialLostPrediction;
                                        definiteImprovementFound = potentialLostPrediction == null;
                                    }
                                    else if (CompareScores(potentialLostPrediction.Score, lostPrediction.Score) < 0)
                                    {
                                        pairable = i - 1;
                                        lostPrediction = potentialLostPrediction;
                                    }
                                }
                            }
                            else
                            {
                                Debug.Assert(!prev.Text.StartsWith(next.Text));
                            }
                        }

                        if (pairable != -1)
                        {
                            if (lostPrediction == null || CompareScores(lostPrediction.Score, nextCorePrediction.Score) < 0)
                            {
                                Debug.Assert(corePredicitions[pairable] == coreCompoundPredictions[pairable][0]);

                                Debug.Assert(coreCompoundPredictions.Count == corePredicitions.Count);
                                Debug.Assert(followOnPredictions.Count == corePredicitions.Count);

                                var targetPredictions = coreCompoundPredictions[pairable];
                                var sourcePredictions = coreCompoundPredictions[pairable + 1];

                                Debug.Assert(targetPredictions[0].Text.Length < sourcePredictions[0].Text.Length);
                                Debug.Assert(sourcePredictions[0].Text.StartsWith(targetPredictions[0].Text));

                                var sourcePosition = 0;
                                var targetPosition = 0;
                                while (sourcePosition < sourcePredictions.Count && targetPosition < targetPredictions.Count)
                                {
                                    if (sourcePredictions[sourcePosition].Index < targetPredictions[targetPosition].Index)
                                    {
                                        Debug.Assert(targetPredictions[targetPosition].Text.StartsWith(sourcePredictions[sourcePosition].Text));
                                        targetPredictions.Insert(targetPosition, sourcePredictions[sourcePosition]);
                                        sourcePosition++;
                                    }
                                    else
                                    {
                                        Debug.Assert(targetPredictions[targetPosition].Index < sourcePredictions[sourcePosition].Index);
                                        Debug.Assert(sourcePredictions[sourcePosition].Text.StartsWith(targetPredictions[targetPosition].Text));
                                    }
                                    targetPosition++;
                                }

                                if (sourcePosition < sourcePredictions.Count)
                                {
                                    while (sourcePosition < sourcePredictions.Count)
                                    {
                                        targetPredictions.Add(sourcePredictions[sourcePosition]);
                                        sourcePosition++;
                                    }

                                    Debug.Assert(ReferenceEquals(lostPrediction, followOnPredictions[pairable]));
                                    followOnPredictions.RemoveAt(pairable);
                                }
                                else
                                {
                                    Debug.Assert(ReferenceEquals(lostPrediction, followOnPredictions[pairable + 1]));
                                    followOnPredictions.RemoveAt(pairable + 1);
                                }

                                coreCompoundPredictions.RemoveAt(pairable + 1);
                                corePredicitions.RemoveAt(pairable + 1);

                                Debug.Assert(coreCompoundPredictions.Count == corePredicitions.Count);
                                Debug.Assert(followOnPredictions.Count == corePredicitions.Count);

                                AddNextPrediction(nextCorePrediction);
                                nextCorePrediction = GetNextCorePrediction(enumerator);

                                Debug.Assert(coreCompoundPredictions.Count == corePredicitions.Count);
                                Debug.Assert(followOnPredictions.Count == corePredicitions.Count);

                                improved = true;
                            }
                        }
                    }

                    Debug.Assert(coreCompoundPredictions.Count == corePredicitions.Count);
                    Debug.Assert(followOnPredictions.Count == corePredicitions.Count);
                }

                DisplayInitialCorePredictions(coreCompoundPredictions, followOnPredictions, nextCorePrediction);
            }

            //var maker = PersistantPredictor.CreatePredictionMaker(this, null, Context);

            for (var compoundPredictionIndex = 0; compoundPredictionIndex < coreCompoundPredictions.Count; compoundPredictionIndex++)
            {
                var compoundPrediction = coreCompoundPredictions[compoundPredictionIndex];

                var length = 0;

                for (var position = 0; position < compoundPrediction.Count; position++)
                {
                    var prediction = compoundPrediction[position];
                    while (length < prediction.Text.Length)
                    {
                        if (char.IsSurrogate(prediction.Text[length]))
                        {
                            length += 2;
                        }
                        else
                        {
                            length++;
                        }

                        if (length < prediction.Text.Length)
                        {
                            var candidate = prediction.Text.Substring(0, length);
                            if (_tokens.TryGetToken(candidate, out var candidateToken))
                            {
                                if (!predictedTokens.Contains(candidateToken))
                                {
                                    var candidateIndex = GetTokenIndex(candidateToken);
                                    if (lowerBound <= candidateIndex && candidateIndex < upperBound &&
                                        TokenFilter.IsTokenVisible(candidateToken))
                                    {
                                        var candidateScore = maker.GetScore(candidateToken);
                                        var candidateText = _tokens[candidateToken];

                                        var candidatePrediction = new WordPrediction(candidateScore, candidateIndex, candidateText);

                                        var insertPosition = 0;
                                        while (insertPosition < compoundPrediction.Count &&
                                            compoundPrediction[insertPosition].Index < candidatePrediction.Index)
                                        {
                                            insertPosition++;
                                        }
                                        compoundPrediction.Insert(insertPosition, candidatePrediction);

                                        predictedTokens.Add(candidateToken);

                                        Debug.WriteLine($"TODO: Should add {candidate} ({candidateScore})");
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine($"Already included found {candidate}");
                                }
                            }
                        }
                    }
                }

                var bubblePosition = compoundPredictionIndex;
                while (0 < bubblePosition &&
                    coreCompoundPredictions[compoundPredictionIndex][0].Index < coreCompoundPredictions[bubblePosition - 1][0].Index)
                {
                    bubblePosition--;
                }

                if (bubblePosition != compoundPredictionIndex)
                {
                    coreCompoundPredictions.RemoveAt(compoundPredictionIndex);
                    coreCompoundPredictions.Insert(bubblePosition, compoundPrediction);
                }
            }

            for (var compoundPredictionIndex = 0; compoundPredictionIndex < coreCompoundPredictions.Count; compoundPredictionIndex++)
            {
                var compoundPrediction = coreCompoundPredictions[compoundPredictionIndex];
                var longestPrediction = compoundPrediction[compoundPrediction.Count - 1];
                var longestPredictionText = longestPrediction.Text;
                var includedPrefixIndex = longestPrediction.Index;
                var beyondPrefixIndex = includedPrefixIndex;
                var limitFound = false;
                for (var step = 1; !limitFound; step += 1)
                {
                    includedPrefixIndex = beyondPrefixIndex;
                    beyondPrefixIndex = includedPrefixIndex + step;
                    if (upperBound <= beyondPrefixIndex)
                    {
                        beyondPrefixIndex = upperBound;
                        limitFound = true;
                    }
                    else
                    {
                        var candidateLimitToken = GetIndexToken(beyondPrefixIndex);
                        var candidateLimitText = _tokens[candidateLimitToken];
                        if (!candidateLimitText.StartsWith(longestPredictionText, StringComparison.OrdinalIgnoreCase))
                        {
                            limitFound = true;
                        }
                    }
                }

                var foundLimit = false;
                do
                {
                    if (includedPrefixIndex + 1 == beyondPrefixIndex)
                    {
                        foundLimit = true;
                    }
                    else
                    {
                        var midIndex = (includedPrefixIndex + beyondPrefixIndex) / 2;
                        var midToken = GetIndexToken(midIndex);
                        var midText = _tokens[midToken];
                        if (midText.StartsWith(longestPredictionText, StringComparison.OrdinalIgnoreCase))
                        {
                            includedPrefixIndex = midIndex;
                        }
                        else
                        {
                            beyondPrefixIndex = midIndex;
                        }
                    }
                }
                while (!foundLimit);

                if (longestPrediction.Index + 1 < beyondPrefixIndex)
                {
                    Debug.WriteLine($"Consider extending {longestPredictionText}:");
                    var followOn = followOnPredictions[compoundPredictionIndex];
                    var additionalScores = maker.GetTopScores(longestPrediction.Index + 1, beyondPrefixIndex, false);
                    using (var enumerator = additionalScores.GetEnumerator())
                    {
                        var extendedPredictionText = longestPredictionText;

                        var improved = true;
                        for (var candidatePrediction = GetNextCorePrediction(enumerator);
                            improved && candidatePrediction != null;
                            candidatePrediction = GetNextCorePrediction(enumerator))
                        {
                            if (candidatePrediction.Text.StartsWith(longestPredictionText))
                            {
                                if (followOn != null && CompareScores(candidatePrediction.Score, followOn.Score) < 0)
                                {
                                    Debug.WriteLine($"\t-{candidatePrediction} less likely than {followOn}");
                                    improved = false;
                                }
                                else if (extendedPredictionText.StartsWith(candidatePrediction.Text))
                                {
                                    Debug.WriteLine($"\t+{candidatePrediction.Text}");
                                    InsertPrediction(compoundPrediction, candidatePrediction);
                                }
                                else if (candidatePrediction.Text.StartsWith(extendedPredictionText))
                                {
                                    Debug.WriteLine($"\t*{candidatePrediction.Text}");
                                    InsertPrediction(compoundPrediction, candidatePrediction);
                                    extendedPredictionText = candidatePrediction.Text;

                                    var followOnMaker = maker.CreateNextPredictionMaker(candidatePrediction.Token, null);
                                    var followOnPrediction = GetTopPrediction(followOnMaker);
                                    followOnPredictions[compoundPredictionIndex] = followOnPrediction;

                                }
                            }
                        }
                    }
                }
            }

            var predictionsList = new SortedList<int, IReadOnlyList<ITile>>();

            for (var position = 0; position < coreCompoundPredictions.Count; position++)
            {
                var predictions = new List<ITile>();
                var coreCompoundPrediction = coreCompoundPredictions[position];
                var headPrediction = coreCompoundPrediction[0];

                var headWord = _tokens.GetString(headPrediction.Token);
                if (headWord[0] == '\0')
                {
                    var command = (TileCommand)Enum.Parse(typeof(TileCommand), headWord.Substring(1));
                    var tile = new CommandItem(LastTile, this, command);
                    predictions.Add(tile);
                }
                else
                {
                    var item = CreateSuggestedWordItem(headWord);
                    predictions.Add(item);

                    for (var corePosition = 1; corePosition < coreCompoundPrediction.Count; corePosition++)
                    {
                        var tailPrediction = coreCompoundPrediction[corePosition];
                        var tailWord = _tokens.GetString(tailPrediction.Token);
                        Debug.Assert(tailWord.StartsWith(headWord));
                        item = new ExtendedSuggestedWordItem(LastTile, this, tailWord, tailWord.Substring(headWord.Length));

                        predictions.Add(item);

                        headWord = tailWord;
                    }

                    var followOn = followOnPredictions[position];
                    if (followOn != null)
                    {
                        var firstCreatedItem = GetNextItem(item, followOn.Token);
                        var newItem = firstCreatedItem as SuggestedWordItem;
                        predictions.Add(firstCreatedItem);

                        var followOnMaker = maker.CreateNextPredictionMaker(followOn.Token, null);
                        var done = newItem == null;
                        while (!done && predictions.Count < maxListItemCount)
                        {
                            var followOnPrediction = GetTopPrediction(followOnMaker);
                            if (followOnPrediction != null)
                            {
                                item = newItem;
                                var createdItem = GetNextItem(item, followOnPrediction.Token);
                                newItem = createdItem as SuggestedWordItem;
                                followOnMaker = followOnMaker.CreateNextPredictionMaker(followOnPrediction.Token, null);
                                predictions.Add(createdItem);

                                if (newItem == null)
                                {
                                    done = true;
                                }
                            }
                            else
                            {
                                done = true;
                            }
                        }

                        // item = newItem;
                    }
                }

                predictionsList.Add(headPrediction.Index, predictions);
            }

            return predictionsList;

            void AddNextPrediction(WordPrediction prediction)
            {
                var added = predictedTokens.Add(prediction.Token);
                Debug.Assert(added);

                var position = 0;
                while (position < corePredicitions.Count && corePredicitions[position].Index < prediction.Index)
                {
                    position++;
                }

                corePredicitions.Insert(position, prediction);

                var compoundCorePrediction = new List<WordPrediction>(1) { prediction };
                coreCompoundPredictions.Insert(position, compoundCorePrediction);

                if (prediction.Text[0] != '\0')
                {
                    var followOnMaker = maker.CreateNextPredictionMaker(prediction.Token, null);

                    var followOnPrediction = GetTopPrediction(followOnMaker);
                    followOnPredictions.Insert(position, followOnPrediction);
                }
                else
                {
                    followOnPredictions.Insert(position, null);
                }
            }

            void InsertPrediction(List<WordPrediction> predictions, WordPrediction prediction)
            {
                predictedTokens.Add(prediction.Token);

                var position = predictions.Count;
                while (0 < position && prediction.Index < predictions[position - 1].Index)
                {
                    position--;
                }

                predictions.Insert(position, prediction);
            }
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

            if (index != _vocabularyList.Count)
            {
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
            }
            else
            {
                value = new InterstitialSpellingItem(LastTile, _spellingSource, index);
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
            DeltaPredictor.Clear(); ;
        }

        internal void RollbackAndAddSequence(IReadOnlyList<int> sequence, int increment)
        {
            PersistantPredictor.Subtract(DeltaPredictor);
            PersistantPredictor.AddSequence(sequence, increment);
        }

        internal void ShowTestCard()
        {
            // Truncate head items.
            for (var i = _headItems.Count - 1; 0 < i; i--)
            {
                _headItems.RemoveAt(i);
            }

            var headWordItem = new HeadWordItem(_headItems[0], this, "Word");
            _headItems.Add(headWordItem);

            var ghostWordItem = new GhostWordItem(_headItems[1], this, "Ghost");
            _headItems.Add(ghostWordItem);

            var ghostStopItem = new GhostStopItem(_headItems[2], this);
            _headItems.Add(ghostStopItem);
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
