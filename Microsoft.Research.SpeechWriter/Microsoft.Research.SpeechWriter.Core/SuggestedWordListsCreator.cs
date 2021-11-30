using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    class SuggestedWordListsCreator
    {
        private readonly WordVocabularySource _source;
        private readonly StringTokens _tokens;
        private readonly ScoredTokenPredictionMaker _maker;
        private readonly Func<int, bool> _isTokenVisible;
        private readonly bool _startOnFirstWord;
        private readonly int _lowerBound;
        private readonly int _upperBound;
        private readonly int _maxListCount;
        private readonly int _maxListItemCount;

        private readonly bool _findFollowOnPredictions;
        private readonly bool _combineCorePredictions;
        private readonly bool _findCorePredictionPrefixes;
        private readonly bool _findCorePredictionSuffixes;

        private readonly HashSet<int> _predictedTokens = new HashSet<int>();

        private readonly List<NascentWordPredictionList> _nascents;

        private readonly Func<string, string> _capitalizer;

        private SuggestedWordListsCreator(WordVocabularySource source,
            StringTokens tokens,
            ScoredTokenPredictionMaker maker,
            Func<int, bool> isTokenVisible,
            bool startOnFirstWord,
            int lowerBound,
            int upperBound,
            int maxListCount,
            int maxListItemCount)
        {
            _source = source;
            _tokens = tokens;
            _maker = maker;
            _isTokenVisible = isTokenVisible;
            _startOnFirstWord = startOnFirstWord;
            _lowerBound = lowerBound;
            _upperBound = upperBound;
            _maxListCount = maxListCount;
            _maxListItemCount = maxListItemCount;

            var settings = source.Model.Environment.Settings;
            _findFollowOnPredictions = settings.FindFollowOnPredictions;
            _combineCorePredictions = settings.CombineCorePredictions;
            _findCorePredictionPrefixes = settings.FindCorePredictionPrefixes;
            _findCorePredictionSuffixes = settings.FindCorePredictionSuffixes;

            _nascents = new List<NascentWordPredictionList>(_maxListCount);

            _capitalizer = source.Model.Environment.TryCapitalizeFirstWord;
        }

        private WordPrediction CreatePrediction(int[] score, bool isFirstWord)
        {
            WordPrediction prediction;

            var token = score[0];
            var index = _source.GetTokenIndex(token);
            var rawText = _tokens[token];
            string casedText;

            var isFollowOnFirstWord = false;
            if (isFirstWord)
            {
                casedText = _capitalizer(rawText);
                if (casedText == null)
                {
                    casedText = rawText;
                }
                else
                {
                    isFollowOnFirstWord = true;
                }
            }
            else
            {
                casedText = rawText;
            }

            Debug.Assert(string.Compare(rawText, casedText, StringComparison.OrdinalIgnoreCase) == 0);

            prediction = new WordPrediction(score, index, rawText, casedText, isFollowOnFirstWord);
            return prediction;
        }

        private WordPrediction GetNextCorePrediction(IEnumerator<int[]> enumerator, bool isFirstWord)
        {
            WordPrediction prediction;

            if (enumerator.MoveNext())
            {
                var score = enumerator.Current;

                prediction = CreatePrediction(score, isFirstWord);
            }
            else
            {
                prediction = null;
            }

            return prediction;
        }

        private WordPrediction GetTopPrediction(ScoredTokenPredictionMaker maker, bool isFirstWord)
        {
            WordPrediction value;

            if (_findFollowOnPredictions)
            {
                var topScores = maker.GetTopScores(0, _source.Count, true, false);
                using (var enumerator = topScores.GetEnumerator())
                {
                    value = GetNextCorePrediction(enumerator, isFirstWord);
                }
            }
            else
            {
                value = null;
            }

            Debug.Assert(value == null || 3 <= value.Score.Length, "Only true following predictions expected");

            return value;
        }

        internal static SortedList<int, IReadOnlyList<ITile>> CreateSuggestionLists(WordVocabularySource source,
            StringTokens tokens,
            ScoredTokenPredictionMaker maker,
            ITokenTileFilter filter,
            bool isFirstWord,
            int lowerBound,
            int upperBound,
            int maxListCount,
            int maxListItemCount)
        {
            var creator = new SuggestedWordListsCreator(source, tokens, maker, filter.IsTokenVisible, isFirstWord, lowerBound, upperBound, maxListCount, maxListItemCount);
            var list = creator.Run();
            return list;
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

        private Func<string, string> GetEncaser(bool isFirstWord)
        {
            Func<string, string> encaser;

            if (isFirstWord)
            {
                Func<string, string> capitalizer = _source.Model.Environment.TryCapitalizeFirstWord;
                encaser = s => capitalizer(s) ?? s;
            }
            else
            {
                encaser = s => s;
            }

            return encaser;
        }

        private void CreateCorePredictions()
        {
            var scores = _maker.GetTopScores(_lowerBound, _upperBound, _isTokenVisible == null, true);

            using (var enumerator = scores.GetEnumerator())
            {
                // Seed predictions with most likely items.
                var nextCorePrediction = GetNextCorePrediction(enumerator, _startOnFirstWord);
                for (var iteration = 0; iteration < _maxListCount && nextCorePrediction != null; iteration++)
                {
                    AddNextPrediction(nextCorePrediction);
                    nextCorePrediction = GetNextCorePrediction(enumerator, _startOnFirstWord);
                }

                // Apply strategies to improve the core predictions.
                for (var improved = _combineCorePredictions; improved;)
                {
                    improved = false;

                    if (1 < _nascents.Count && nextCorePrediction != null)
                    {
                        var pairable = -1;
                        var lostPrediction = (WordPrediction)null;

                        var next = _nascents[0];
                        var definiteImprovementFound = false;
                        for (var i = 1; !definiteImprovementFound && i < _nascents.Count; i++)
                        {
                            var prev = next;
                            next = _nascents[i];

                            if (prev.CanMergeWithNext(next))
                            {
                                var potentialLostPrediction = prev._followOn;

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

                        if (pairable != -1 &&
                            (lostPrediction == null || CompareScores(lostPrediction.Score, nextCorePrediction.Score) < 0))
                        {
                            _nascents[pairable].MergeWithNext(_nascents[pairable + 1]);
                            _nascents.RemoveAt(pairable + 1);

                            AddNextPrediction(nextCorePrediction);
                            nextCorePrediction = GetNextCorePrediction(enumerator, _startOnFirstWord);

                            improved = true;
                        }
                    }
                }
            }
        }

        private void CreatePrefixPredictions()
        {
            for (var compoundPredictionIndex = 0; compoundPredictionIndex < _nascents.Count; compoundPredictionIndex++)
            {
                var compoundPrediction = _nascents[compoundPredictionIndex]._list;

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
                                if (!_predictedTokens.Contains(candidateToken))
                                {
                                    var candidateIndex = _source.GetTokenIndex(candidateToken);
                                    if (_lowerBound <= candidateIndex && candidateIndex < _upperBound &&
                                        _isTokenVisible(candidateToken))
                                    {
                                        var candidateScore = _maker.GetScore(candidateToken);
                                        var candidateText = _tokens[candidateToken];

                                        var candidatePrediction = CreatePrediction(candidateScore, _startOnFirstWord);

                                        var insertPosition = 0;
                                        while (insertPosition < compoundPrediction.Count &&
                                            compoundPrediction[insertPosition].Index < candidatePrediction.Index)
                                        {
                                            insertPosition++;
                                        }
                                        compoundPrediction.Insert(insertPosition, candidatePrediction);

                                        _predictedTokens.Add(candidateToken);

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
                    _nascents[compoundPredictionIndex]._list[0].Index < _nascents[bubblePosition - 1]._list[0].Index)
                {
                    bubblePosition--;
                }

                if (bubblePosition != compoundPredictionIndex)
                {
                    var nascent = _nascents[compoundPredictionIndex];
                    _nascents.RemoveAt(compoundPredictionIndex);

                    _nascents.Insert(bubblePosition, nascent);
                }
            }
        }

        private void CreateSuffixPredictions()
        {
            for (var compoundPredictionIndex = _nascents.Count - 1; 0 <= compoundPredictionIndex; compoundPredictionIndex--)
            {
                var compoundPrediction = _nascents[compoundPredictionIndex]._list;
                var longestPrediction = compoundPrediction[compoundPrediction.Count - 1];
                var longestPredictionText = longestPrediction.Text;
                var includedPrefixIndex = longestPrediction.Index;
                var beyondPrefixIndex = includedPrefixIndex;
                var limitFound = false;

                for (var step = 1; !limitFound; step += 1)
                {
                    includedPrefixIndex = beyondPrefixIndex;
                    beyondPrefixIndex = includedPrefixIndex + step;
                    if (_upperBound <= beyondPrefixIndex)
                    {
                        beyondPrefixIndex = _upperBound;
                        limitFound = true;
                    }
                    else
                    {
                        var candidateLimitToken = _source.GetIndexToken(beyondPrefixIndex);
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
                        var midToken = _source.GetIndexToken(midIndex);
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
                    var capitalizer = GetEncaser(longestPrediction.IsFollowOnFirstWord);

                    Debug.WriteLine($"Consider extending {longestPredictionText}:");
                    var followOn = _nascents[compoundPredictionIndex]._followOn;

                    var minSuffixIndex = longestPrediction.Index + 1;
                    var limSuffixIndex = beyondPrefixIndex;
                    var extendedPredictionText = longestPredictionText;

                    bool SuffixFilter(int token)
                    {
                        var index = _source.GetTokenIndex(token);
                        var value = minSuffixIndex <= index && index < limSuffixIndex && !_predictedTokens.Contains(token);

                        if (value)
                        {
                            var text = capitalizer(_tokens[token]);
                            value = extendedPredictionText.StartsWith(text) || text.StartsWith(extendedPredictionText);
                        }

                        return value;
                    }

                    var additionalScores = _maker.GetTopScores(SuffixFilter, true);
                    using (var enumerator = additionalScores.GetEnumerator())
                    {
                        var improved = true;
                        for (var candidatePrediction = GetNextCorePrediction(enumerator, _startOnFirstWord);
                            improved && candidatePrediction != null;
                            candidatePrediction = GetNextCorePrediction(enumerator, _startOnFirstWord))
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

                                    var followOnMaker = _maker.CreateNextPredictionMaker(candidatePrediction.Token, null);
                                    var followOnPrediction = GetTopPrediction(followOnMaker, longestPrediction.IsFollowOnFirstWord);
                                    _nascents[compoundPredictionIndex]._followOn = followOnPrediction;

                                }
                            }
                        }
                    }
                }
            }
        }

        private SortedList<int, IReadOnlyList<ITile>> CreateFinalPredictionsList()
        {
            var predictionsList = new SortedList<int, IReadOnlyList<ITile>>();

            for (var position = 0; position < _nascents.Count; position++)
            {
                var predictions = new List<ITile>();
                var coreCompoundPrediction = _nascents[position]._list;
                var headPrediction = coreCompoundPrediction[0];

                var headWord = headPrediction.RawText;
                if (headWord[0] == '\0')
                {
                    var command = (TileCommand)Enum.Parse(typeof(TileCommand), headWord.Substring(1));
                    var tile = new CommandItem(_source.Model.LastTile, _source, command);
                    predictions.Add(tile);
                }
                else
                {
                    var item = _source.CreateSuggestedWordItem(headWord);
                    predictions.Add(item);

                    for (var corePosition = 1; corePosition < coreCompoundPrediction.Count; corePosition++)
                    {
                        var tailPrediction = coreCompoundPrediction[corePosition];
                        var tailWord = tailPrediction.RawText;
                        Debug.Assert(tailWord.StartsWith(headWord, StringComparison.OrdinalIgnoreCase));
                        if (!headWord.StartsWith(tailWord))
                        {
                            item = new ExtendedSuggestedWordItem(_source.Model.LastTile, _source, tailWord, tailWord.Substring(headWord.Length));

                            predictions.Add(item);

                            headWord = tailWord;
                        }
                        else
                        {
                            Debug.WriteLine($"Unexpected situation of non-equal words being each others prefix: {headWord} + {tailWord}");
                        }
                    }

                    var followOn = _nascents[position]._followOn;
                    if (followOn != null)
                    {
                        var firstCreatedItem = _source.GetNextItem(item, followOn.Token);
                        var newItem = firstCreatedItem as SuggestedWordItem;
                        predictions.Add(firstCreatedItem);

                        var followOnMaker = _maker.CreateNextPredictionMaker(followOn.Token, null);
                        var isFollowOnFirstWord = followOn.IsFollowOnFirstWord;
                        var done = newItem == null;
                        while (!done && predictions.Count < _maxListItemCount)
                        {
                            var followOnPrediction = GetTopPrediction(followOnMaker, isFollowOnFirstWord);
                            if (followOnPrediction != null)
                            {
                                item = newItem;
                                var createdItem = _source.GetNextItem(item, followOnPrediction.Token);
                                newItem = createdItem as SuggestedWordItem;
                                followOnMaker = followOnMaker.CreateNextPredictionMaker(followOnPrediction.Token, null);
                                predictions.Add(createdItem);
                                isFollowOnFirstWord = followOnPrediction.IsFollowOnFirstWord;

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
        }

        private void AddNextPrediction(WordPrediction prediction)
        {
            var added = _predictedTokens.Add(prediction.Token);
            Debug.Assert(added);

            var position = 0;
            while (position < _nascents.Count && _nascents[position]._first.Index < prediction.Index)
            {
                position++;
            }

            WordPrediction followOn;
            if (_findFollowOnPredictions && prediction.Text[0] != '\0')
            {
                var followOnMaker = _maker.CreateNextPredictionMaker(prediction.Token, null);
                followOn = GetTopPrediction(followOnMaker, prediction.IsFollowOnFirstWord);
            }
            else
            {
                followOn = null;
            }

            var nascent = new NascentWordPredictionList(prediction, followOn);

            _nascents.Insert(position, nascent);
        }

        private void InsertPrediction(List<WordPrediction> predictions, WordPrediction prediction)
        {
            _predictedTokens.Add(prediction.Token);

            var position = predictions.Count;
            while (0 < position && prediction.Index < predictions[position - 1].Index)
            {
                position--;
            }

            predictions.Insert(position, prediction);
        }

        private SortedList<int, IReadOnlyList<ITile>> Run()
        {
            CreateCorePredictions();

            if (_findCorePredictionPrefixes)
            {
                CreatePrefixPredictions();
            }

            if (_findCorePredictionSuffixes)
            {
                CreateSuffixPredictions();
            }

            var predictionsList = CreateFinalPredictionsList();

            return predictionsList;
        }
    }
}
