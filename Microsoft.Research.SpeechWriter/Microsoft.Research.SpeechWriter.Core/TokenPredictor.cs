using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Token sequence predictor.
    /// </summary>
    public class TokenPredictor
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

                info.IncrementCount(increment);

                for (var index = start + 1; index < limit; index++)
                {
                    database = info.GetChildren();
                    info = database.GetValue(sequence[index]);

                    info.IncrementCount(increment);
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
                var info = _database.GetValue(sequence[startPos]);

                for (var index = startPos + 1; index < sequenceCount; index++)
                {
                    info = info.GetChildInfo(sequence[index]);
                }
                info.IncrementCount(increment);
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
                info.IncrementCount(pair.Value.Count);

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
                if (0 < info.DecrementCount(pair.Value.Count))
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

            for (var scanStart = contextStart; result == -1 && scanStart < contextLimit; scanStart++)
            {
                var leafDatabase = _database;

                for (var index = scanStart; leafDatabase != null && index < contextLimit; index++)
                {
                    if (leafDatabase.TryGetValue(context[index], out var info))
                    {
                        leafDatabase = info.GetChildren();
                    }
                    else
                    {
                        leafDatabase = null;
                    }
                }

                if (leafDatabase != null)
                {
                    var maxCount = 0;
                    var tokens = new HashSet<int>();

                    foreach (var pair in leafDatabase)
                    {
                        if (maxCount <= pair.Value.Count)
                        {
                            if (maxCount < pair.Value.Count)
                            {
                                maxCount = pair.Value.Count;
                                tokens.Clear();
                            }

                            tokens.Add(pair.Key);
                        }
                    }

                    if (1 < tokens.Count)
                    {
                        // TODO: This should pick the most used in context, but for the moment do nothing.
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
            }

            return result;
        }

        internal IEnumerable<int> GetTopIndices(PredictiveVocabularySource source, int[] context, int minIndex, int limIndex, int count)
        {
            var toFindCount = count;
            var foundTokens = new HashSet<int>();

            var contextLimit = context.Length;
            var contextStart = Math.Max(0, contextLimit - _width + 1);

            for (var scanIndex = contextStart; toFindCount != 0 && scanIndex <= contextLimit; scanIndex++)
            {
                var dictionaryX = _database;
                for (var index = scanIndex; dictionaryX != null && index < contextLimit; index++)
                {
                    if (dictionaryX.TryGetValue(context[index], out var infoX))
                    {
                        dictionaryX = infoX.GetChildren();
                    }
                    else
                    {
                        dictionaryX = null;
                    }
                }

                if (dictionaryX != null)
                {
                    var candidates = new List<Tuple<int, int>>();

                    var acceptedMin = int.MinValue;
                    foreach (var pair in dictionaryX)
                    {
                        if (!foundTokens.Contains(pair.Key))
                        {
                            var index = source.GetTokenIndex(pair.Key);
                            if (minIndex <= index && index < limIndex && acceptedMin <= pair.Value.Count)
                            {
                                var candidateLimit = candidates.Count;
                                while (0 < candidateLimit && candidates[candidateLimit - 1].Item2 < pair.Value.Count)
                                {
                                    candidateLimit--;
                                }

                                candidates.Insert(candidateLimit, new Tuple<int, int>(pair.Key, pair.Value.Count));

                                if (toFindCount == candidates.Count)
                                {
                                    acceptedMin = candidates[candidates.Count - 1].Item2;
                                }

                                if (toFindCount < candidates.Count &&
                                    candidates[candidates.Count - 1].Item2 < candidates[toFindCount - 1].Item2)
                                {
                                    Debug.Assert(candidates[toFindCount].Item2 < candidates[toFindCount - 1].Item2);
                                    candidates.RemoveRange(toFindCount, candidates.Count - toFindCount);
                                    acceptedMin = candidates[candidates.Count - 1].Item2;
                                }
                            }
                        }
                    }

                    var sortableCandidates = new List<int[]>();
                    foreach (var candidate in candidates)
                    {
                        var counts = new int[contextLimit - scanIndex + 2];
                        counts[0] = candidate.Item2;
                        counts[contextLimit - scanIndex + 1] = candidate.Item1;
                        sortableCandidates.Add(counts);
                    }
                    for (var subIndex = scanIndex + 1; subIndex <= contextLimit; subIndex++)
                    {
                        var dictionary = _database;
                        for (var subSubIndex = subIndex; dictionary != null && subSubIndex < contextLimit; subSubIndex++)
                        {
                            var subInfo = dictionary.GetValue(context[subSubIndex]);
                            dictionary = subInfo.TryGetChildren();
                        }
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

                        yield return index;
                    }
                    toFindCount -= sliceCount;
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
                                toFindCount--;
                                yield return index;
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
            var saved = new SortedDictionary<string, object>();

            saved.Add("#", info.Count);

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
