using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Token sequence predictor.
    /// </summary>
    public sealed class TokenPredictor
    {
        private readonly int _width;

        private readonly TokenPredictorDatabase _database = new TokenPredictorDatabase();

        internal TokenPredictor(int width)
        {
            _width = width;
        }

        internal IEnumerable<int> Tokens => _database.Keys;

        internal void Clear()
        {
            _database.Clear();
        }

        internal TokenPredictor CreateEmpty()
        {
            var predictor = new TokenPredictor(_width);
            return predictor;
        }

        internal TokenPredictor CreateCopy()
        {
            var predictor = CreateEmpty();
            predictor.Add(this);
            return predictor;
        }

        internal void AddSequence(IReadOnlyList<int> sequence, int increment)
        {
            for (var start = 0; start < sequence.Count; start++)
            {
                var limit = Math.Min(start + _width, sequence.Count);

                var database = _database;
                var info = database.GetValue(sequence[start]);

                database.IncrementCount(info, increment);

                for (var index = start + 1; index < limit; index++)
                {
                    database = info.GetChildren();
                    info = database.GetValue(sequence[index]);

                    database.IncrementCount(info, increment);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="increment">Signed increment to apply.</param>
        public void AddSequenceTail(IReadOnlyList<int> sequence, int increment)
        {
            var sequenceCount = sequence.Count;
            Debug.Assert(0 < sequenceCount);

            for (var startPos = Math.Max(0, sequenceCount - _width); startPos < sequenceCount; startPos++)
            {
                var database = _database;
                var info = database.GetValue(sequence[startPos]);

                for (var index = startPos + 1; index < sequenceCount; index++)
                {
                    database = info.GetChildren();
                    info = database.GetValue(sequence[index]);
                }
                database.IncrementCount(info, increment);
            }
        }

        internal void Add(TokenPredictor predictor)
        {
            AddTokens(_database, predictor._database);
        }

        private static void AddTokens(TokenPredictorDatabase accumulator, TokenPredictorDatabase additive)
        {
            foreach (var pair in additive)
            {
                var info = accumulator.GetValue(pair.Key);
                accumulator.IncrementCount(info, pair.Value.Count);

                var additiveChlidren = pair.Value.TryGetChildren();
                if (additiveChlidren != null)
                {
                    AddTokens(info.GetChildren(), additiveChlidren);
                }
            }
        }

        internal void Subtract(TokenPredictor predictor)
        {
            SubtractTokens(_database, predictor._database);
        }

        private static void SubtractTokens(TokenPredictorDatabase accumulator, TokenPredictorDatabase subtractive)
        {
            var expired = default(List<int>);

            foreach (var pair in subtractive)
            {
                var info = accumulator.GetValue(pair.Key);
                if (0 < accumulator.DecrementCount(info, pair.Value.Count))
                {
                    var subtractiveChlidren = pair.Value.TryGetChildren();
                    if (subtractiveChlidren != null)
                    {
                        SubtractTokens(info.GetChildren(), subtractiveChlidren);
                    }
                }
                else
                {
                    Debug.Assert(info.Count == 0);

                    if (expired == null)
                    {
                        expired = new List<int>();
                    }
                    expired.Add(pair.Key);
                }
            }

            if (expired != null)
            {
                foreach (var token in expired)
                {
                    accumulator.Remove(token);
                }
            }
        }

        internal int GetTopToken(int[] context)
        {
            var result = -1;

            var contextLimit = context.Length;
            var contextStart = Math.Max(0, contextLimit - _width + 1);

            var scanStart = contextStart;
            TokenPredictorDatabase leafDatabase = null;
            for (; leafDatabase == null && scanStart < contextLimit; scanStart++)
            {
                leafDatabase = _database.GetChild(context, scanStart, contextLimit - scanStart);
            }

            if (leafDatabase != null)
            {
                var tokens = leafDatabase.GetTopRanked();

                for (; 1 < tokens.Count && scanStart < contextLimit; scanStart++)
                {
                    leafDatabase = _database.GetChild(context, scanStart, contextLimit - scanStart);
                    Debug.Assert(leafDatabase != null);

                    tokens = leafDatabase.GetTopRanked(tokens);
                }

                Debug.Assert(result == -1);
                foreach (var token in tokens)
                {
                    if (result < token)
                    {
                        result = token;
                    }
                }
            }

            return result;
        }

        private struct CandidatePair
        {
            internal CandidatePair(int token, int count)
            {
                Token = token;
                Count = count;
            }

            internal int Token { get; private set; }
            internal int Count { get; private set; }
        };

        internal ScoredTokenPredictionMaker<T> CreatePredictionMaker<T>(PredictiveVocabularySource<T> source, ITokenTileFilter filter, int[] context)
            where T : ISuggestionItem
        {
            var maker = new ScoredTokenPredictionMaker<T>(source, _database, filter, context);
            return maker;
        }

        internal IEnumerable<int[]> GetTopScores<T>(PredictiveVocabularySource<T> source, ITokenTileFilter filter, int[] context, int minIndex, int limIndex)
            where T : ISuggestionItem
        {
            var scores = ScoredTokenPredictionMaker<T>.GetTopScores(source, _database, filter, context, minIndex, limIndex);
            return scores;
        }

        internal IEnumerable<int> GetTopIndices<T>(PredictiveVocabularySource<T> source, ITokenTileFilter filter, int[] context, int minIndex, int limIndex, int count)
            where T : ISuggestionItem
        {
            var toFindCount = count;
            var foundTokens = new HashSet<int>();

            var contextLimit = context.Length;
            var contextStart = Math.Max(0, contextLimit - _width + 1);

            var scanIndex = contextStart;
            while (toFindCount != 0 && scanIndex <= contextLimit)
            {
                var processed = true;
                var database = _database.GetChild(context, scanIndex, contextLimit - scanIndex);

                if (database != null)
                {
                    var candidates = new List<CandidatePair>();

                    var acceptedMin = int.MinValue;
                    foreach (var pair in database)
                    {
                        var token = pair.Key;

                        if (!foundTokens.Contains(token))
                        {
                            var tokenCount = pair.Value.Count;

                            var index = source.GetTokenIndex(token);
                            if (minIndex <= index && index < limIndex && acceptedMin <= tokenCount)
                            {
                                var candidateLimit = candidates.Count;
                                while (0 < candidateLimit && candidates[candidateLimit - 1].Count < tokenCount)
                                {
                                    candidateLimit--;
                                }

                                candidates.Insert(candidateLimit, new CandidatePair(token, tokenCount));

                                if (toFindCount == candidates.Count)
                                {
                                    acceptedMin = candidates[candidates.Count - 1].Count;
                                }

                                if (toFindCount < candidates.Count &&
                                    candidates[candidates.Count - 1].Count < candidates[toFindCount - 1].Count)
                                {
                                    Debug.Assert(candidates[toFindCount].Count < candidates[toFindCount - 1].Count);
                                    candidates.RemoveRange(toFindCount, candidates.Count - toFindCount);
                                    acceptedMin = candidates[candidates.Count - 1].Count;
                                }
                            }
                        }
                    }

                    var sortableCandidates = new List<int[]>();
                    foreach (var candidate in candidates)
                    {
                        var counts = new int[contextLimit - scanIndex + 2];
                        counts[0] = candidate.Count;
                        counts[contextLimit - scanIndex + 1] = candidate.Token;
                        sortableCandidates.Add(counts);
                    }
                    for (var subIndex = scanIndex + 1; subIndex <= contextLimit; subIndex++)
                    {
                        var dictionary = _database.GetChild(context, subIndex, contextLimit - subIndex);

                        if (dictionary != null)
                        {
                            foreach (var counts in sortableCandidates)
                            {
                                Debug.Assert(counts[subIndex - scanIndex] == 0);

                                if (dictionary.TryGetValue(counts[counts.Length - 1], out var subCount))
                                {
                                    counts[subIndex - scanIndex] = subCount.Count;
                                }
                            }
                        }
                    }
                    sortableCandidates.Sort(RankSort);

                    var sliceCount = Math.Min(sortableCandidates.Count, toFindCount);
                    for (var i = 0; i < sliceCount; i++)
                    {
                        var counts = sortableCandidates[i];
                        var token = counts[counts.Length - 1];
                        var index = source.GetTokenIndex(token);

                        foundTokens.Add(token);

                        if (filter.IsTokenVisible(token))
                        {
                            yield return index;
                            toFindCount--;
                        }
                        else
                        {
                            processed = false;
                        }
                    }
                }

                if (processed)
                {
                    scanIndex++;
                }
            }

            if (0 < toFindCount)
            {
                var tokens = source.GetTokens();

                using (var enumerator = tokens.GetEnumerator())
                {
                    while (0 < toFindCount && enumerator.MoveNext())
                    {
                        var token = enumerator.Current;

                        if (!foundTokens.Contains(token))
                        {
                            var index = source.GetTokenIndex(token);
                            if (minIndex <= index && index < limIndex)
                            {
                                foundTokens.Add(token);

                                if (filter.IsTokenVisible(token))
                                {
                                    yield return index;
                                    toFindCount--;
                                }
                            }
                        }
                    }
                }
            }

            int RankSort(int[] l, int[] r)
            {
                Debug.Assert(l.Length == r.Length);
                var lim = l.Length;

                var i = 0;
                while (i < lim && l[i] == r[i])
                {
                    i++;
                }
                var comparison = i == lim ? 0 : r[i].CompareTo(l[i]);
                return comparison;
            }
        }

        internal IDictionary<string, object> ToJsonDictionary(StringTokens tokens)
        {
            var json = ToJsonDictionary(tokens, _database);
            return json;
        }

        private static IDictionary<string, object> ToJsonDictionary(StringTokens tokens, TokenPredictorDatabase database)
        {
            var json = new SortedDictionary<string, object>();

            foreach (var pair in database)
            {
                var key = pair.Key == 0 ? string.Empty : tokens.GetString(pair.Key);
                var value = ToJsonDictionary(tokens, pair.Value);
                json.Add(key, value);
            }

            return json;
        }

        private static IDictionary<string, object> ToJsonDictionary(StringTokens tokens, TokenPredictorInfo info)
        {
            var saved = new SortedDictionary<string, object>
            {
                { "#", info.Count }
            };

            var children = info.TryGetChildren();
            if (children != null)
            {
                var value = ToJsonDictionary(tokens, children);
                saved.Add("~", value);
            }

            return saved;
        }
    }
}
