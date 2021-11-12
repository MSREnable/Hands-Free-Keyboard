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
        private readonly int _lowerBound;
        private readonly int _upperBound;
        private readonly int _maxListCount;
        private readonly int _maxListItemCount;

        private readonly bool _findFollowOnPredictions;
        private readonly bool _combineCorePredictions;
        private readonly bool _findCorePredictionPrefixes;
        private readonly bool _findCorePredictionSuffixes;

        private SuggestedWordListsCreator(WordVocabularySource source,
            StringTokens tokens,
            ScoredTokenPredictionMaker maker,
            Func<int, bool> isTokenVisible,
            int lowerBound,
            int upperBound,
            int maxListCount,
            int maxListItemCount)
        {
            _source = source;
            _tokens = tokens;
            _maker = maker;
            _isTokenVisible = isTokenVisible;
            _lowerBound = lowerBound;
            _upperBound = upperBound;
            _maxListCount = maxListCount;
            _maxListItemCount = maxListItemCount;

            var settings = source.Model.Environment.Settings;
            _findFollowOnPredictions = settings.FindFollowOnPredictions;
            _combineCorePredictions = settings.CombineCorePredictions;
            _findCorePredictionPrefixes = settings.FindCorePredictionPrefixes;
            _findCorePredictionSuffixes = settings.FindCorePredictionSuffixes;
        }

        private WordPrediction GetNextCorePrediction(IEnumerator<int[]> enumerator)
        {
            WordPrediction prediction;

            if (enumerator.MoveNext())
            {
                var score = enumerator.Current;
                var token = score[0];
                var index = _source.GetTokenIndex(token);
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

            if (_findFollowOnPredictions)
            {
                var topScores = maker.GetTopScores(0, _source.Count, true, false);
                using (var enumerator = topScores.GetEnumerator())
                {
                    value = GetNextCorePrediction(enumerator);
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
            int lowerBound,
            int upperBound,
            int maxListCount,
            int maxListItemCount)
        {
            var creator = new SuggestedWordListsCreator(source, tokens, maker, filter.IsTokenVisible, lowerBound, upperBound, maxListCount, maxListItemCount);
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

        private SortedList<int, IReadOnlyList<ITile>> Run()
        {
            var scores = _maker.GetTopScores(_lowerBound, _upperBound, _isTokenVisible == null, true);

            var corePredicitions = new List<WordPrediction>(_maxListCount);
            var coreCompoundPredictions = new List<List<WordPrediction>>(_maxListCount);

            var followOnPredictions = new List<WordPrediction>(_maxListCount);

            var predictedTokens = new HashSet<int>();

            using (var enumerator = scores.GetEnumerator())
            {
                // Seed predictions with most likely items.
                var nextCorePrediction = GetNextCorePrediction(enumerator);
                for (var iteration = 0; iteration < _maxListCount && nextCorePrediction != null; iteration++)
                {
                    AddNextPrediction(nextCorePrediction);
                    nextCorePrediction = GetNextCorePrediction(enumerator);
                }

                // Apply strategies to improve the core predictions.
                for (var improved = _combineCorePredictions; improved;)
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
            }

            if (_findCorePredictionPrefixes)
            {
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
                                        var candidateIndex = _source.GetTokenIndex(candidateToken);
                                        if (_lowerBound <= candidateIndex && candidateIndex < _upperBound &&
                                            _isTokenVisible(candidateToken))
                                        {
                                            var candidateScore = _maker.GetScore(candidateToken);
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
            }

            if (_findCorePredictionSuffixes)
            {
                for (var compoundPredictionIndex = coreCompoundPredictions.Count - 1; 0 <= compoundPredictionIndex; compoundPredictionIndex--)
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
                        Debug.WriteLine($"Consider extending {longestPredictionText}:");
                        var followOn = followOnPredictions[compoundPredictionIndex];

                        var minSuffixIndex = longestPrediction.Index + 1;
                        var limSuffixIndex = beyondPrefixIndex;
                        var extendedPredictionText = longestPredictionText;

                        bool SuffixFilter(int token)
                        {
                            var index = _source.GetTokenIndex(token);
                            var value = minSuffixIndex <= index && index < limSuffixIndex && !predictedTokens.Contains(token);

                            if (value)
                            {
                                var text = _tokens[token];
                                value = extendedPredictionText.StartsWith(text) || text.StartsWith(extendedPredictionText);
                            }

                            return value;
                        }

                        var additionalScores = _maker.GetTopScores(SuffixFilter, true);
                        using (var enumerator = additionalScores.GetEnumerator())
                        {
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

                                        var followOnMaker = _maker.CreateNextPredictionMaker(candidatePrediction.Token, null);
                                        var followOnPrediction = GetTopPrediction(followOnMaker);
                                        followOnPredictions[compoundPredictionIndex] = followOnPrediction;

                                    }
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
                        var tailWord = _tokens.GetString(tailPrediction.Token);
                        Debug.Assert(tailWord.StartsWith(headWord));
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

                    var followOn = followOnPredictions[position];
                    if (followOn != null)
                    {
                        var firstCreatedItem = _source.GetNextItem(item, followOn.Token);
                        var newItem = firstCreatedItem as SuggestedWordItem;
                        predictions.Add(firstCreatedItem);

                        var followOnMaker = _maker.CreateNextPredictionMaker(followOn.Token, null);
                        var done = newItem == null;
                        while (!done && predictions.Count < _maxListItemCount)
                        {
                            var followOnPrediction = GetTopPrediction(followOnMaker);
                            if (followOnPrediction != null)
                            {
                                item = newItem;
                                var createdItem = _source.GetNextItem(item, followOnPrediction.Token);
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
                    var followOnMaker = _maker.CreateNextPredictionMaker(prediction.Token, null);

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
    }
}
