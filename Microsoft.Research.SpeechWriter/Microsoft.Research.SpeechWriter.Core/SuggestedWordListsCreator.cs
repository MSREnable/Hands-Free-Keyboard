using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Research.SpeechWriter.Core
{
    class SuggestedWordListsCreator
    {
        private readonly WordVocabularySource source;
        private readonly StringTokens _tokens;
        private readonly ScoredTokenPredictionMaker maker;
        private readonly ITokenTileFilter filter;
        private readonly int lowerBound;
        private readonly int upperBound;
        private readonly int maxListCount;
        private readonly int maxListItemCount;

        private SuggestedWordListsCreator(WordVocabularySource source,
            StringTokens tokens,
            ScoredTokenPredictionMaker maker,
            ITokenTileFilter filter,
            int lowerBound,
            int upperBound,
            int maxListCount,
            int maxListItemCount)
        {
            this.source = source;
            _tokens = tokens;
            this.maker = maker;
            this.filter = filter;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            this.maxListCount = maxListCount;
            this.maxListItemCount = maxListItemCount;
        }

        private WordPrediction GetNextCorePrediction(IEnumerator<int[]> enumerator)
        {
            WordPrediction prediction;

            if (enumerator.MoveNext())
            {
                var score = enumerator.Current;
                var token = score[0];
                var index = source.GetTokenIndex(token);
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

            var topScores = maker.GetTopScores(0, source.Count, true);
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

        internal static SortedList<int, IReadOnlyList<ITile>> CreateSuggestionLists(WordVocabularySource source,
            StringTokens tokens,
            ScoredTokenPredictionMaker maker,
            ITokenTileFilter filter,
            int lowerBound,
            int upperBound,
            int maxListCount,
            int maxListItemCount)
        {
            var creator = new SuggestedWordListsCreator(source, tokens, maker, filter, lowerBound, upperBound, maxListCount, maxListItemCount);
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
            var scores = maker.GetTopScores(lowerBound, upperBound, filter == null);

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
                                    var candidateIndex = source.GetTokenIndex(candidateToken);
                                    if (lowerBound <= candidateIndex && candidateIndex < upperBound &&
                                        filter.IsTokenVisible(candidateToken))
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
                        var candidateLimitToken = source.GetIndexToken(beyondPrefixIndex);
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
                        var midToken = source.GetIndexToken(midIndex);
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
                    var tile = new CommandItem(source.Model.LastTile, source, command);
                    predictions.Add(tile);
                }
                else
                {
                    var item = source.CreateSuggestedWordItem(headWord);
                    predictions.Add(item);

                    for (var corePosition = 1; corePosition < coreCompoundPrediction.Count; corePosition++)
                    {
                        var tailPrediction = coreCompoundPrediction[corePosition];
                        var tailWord = _tokens.GetString(tailPrediction.Token);
                        Debug.Assert(tailWord.StartsWith(headWord));
                        item = new ExtendedSuggestedWordItem(source.Model.LastTile, source, tailWord, tailWord.Substring(headWord.Length));

                        predictions.Add(item);

                        headWord = tailWord;
                    }

                    var followOn = followOnPredictions[position];
                    if (followOn != null)
                    {
                        var firstCreatedItem = source.GetNextItem(item, followOn.Token);
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
                                var createdItem = source.GetNextItem(item, followOnPrediction.Token);
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
    }
}
