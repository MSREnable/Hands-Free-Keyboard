using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    class ScoredTokenPredictionMaker<T> : IComparer<int[]>
        where T : ISuggestionItem
    {
        private readonly PredictiveVocabularySource<T> _source;
        private readonly TokenPredictorDatabase _database;
        private readonly ITokenTileFilter _filter;
        private readonly TokenPredictorDatabase[] _contextDatabases;
        private readonly int _minIndex;
        private readonly int _limIndex;

        internal ScoredTokenPredictionMaker(PredictiveVocabularySource<T> source, TokenPredictorDatabase database, ITokenTileFilter filter, int[] context, int minIndex, int limIndex)
        {
            _source = source;
            _database = database;
            _filter = filter;

            _contextDatabases = GetContextDatabases(context);

            _minIndex = minIndex;
            _limIndex = limIndex;
        }

        private TokenPredictorDatabase GetContextDatabase(int[] context, int contextStart)
        {
            var contextLength = context.Length;
            var database = _database;

            for (var index = contextStart; database != null && index < contextLength; index++)
            {
                if (database.TryGetValue(context[index], out var info))
                {
                    database = info.TryGetChildren();
                }
                else
                {
                    database = null;
                }
            }

            return database;
        }

        private TokenPredictorDatabase[] GetContextDatabases(int[] context)
        {
            var contextLength = context.Length;
            var databases = new List<TokenPredictorDatabase>(contextLength + 1) { _database };

            var done = false;
            for (var index = 1; index <= contextLength && !done; index++)
            {
                var database = GetContextDatabase(context, contextLength - index);
                if (database != null)
                {
                    databases.Add(database);
                }
                else
                {
                    done = true;
                }
            }

            return databases.ToArray();
        }

        internal TokenPredictorDatabase[] GetNextContextDatabases(TokenPredictorDatabase[] previous, int token)
        {
            var previousLength = previous.Length;
            var databases = new List<TokenPredictorDatabase>(previousLength + 1) { _database };

            var done = false;
            for (var index = 0; index < previousLength && !done; index++)
            {
                if (previous[index - 1].TryGetValue(token, out var info))
                {
                    var database = info.TryGetChildren();

                    if (database != null)
                    {
                        databases.Add(database);
                    }
                    else
                    {
                        done = true;
                    }
                }
                else
                {
                    done = true;
                }
            }

            return databases.ToArray();
        }

        internal int[] GetScore(int token)
        {
            var score = new int[_contextDatabases.Length + 1];
            score[0] = token;

            var depth = 0;
            var done = false;
            while (!done && depth < _contextDatabases.Length)
            {
                if (_contextDatabases[depth].TryGetValue(token, out var depthInfo) && depthInfo.Count != 0)
                {
                    score[depth + 1] = depthInfo.Count;
                    depth++;
                }
                else
                {
                    done = true;
                }
            }

            Array.Resize(ref score, depth + 1);

            return score;
        }

        internal IEnumerable<int[]> GetTopScores(bool unfiltered)
        {
            /// <summary>
            /// Tokens already found and returned.
            /// </summary>
            HashSet<int> _foundTokens = new HashSet<int>();

            bool IsNewToken(int token)
            {
                var value = unfiltered;

                if (!value && !_foundTokens.Contains(token))
                {
                    var index = _source.GetTokenIndex(token);
                    if (_minIndex <= index && index < _limIndex)
                    {
                        _foundTokens.Add(token);

                        if (_filter.IsTokenVisible(token))
                        {
                            value = true;
                        }
                        else
                        {
                            value = false;
                        }
                    }
                    else
                    {
                        value = false;
                    }
                }

                return value;
            }

            // Produce scores with three or more elements - those that may not entirely be ordered
            // by their final ordinal.
            for (var index = _contextDatabases.Length - 1; 1 <= index; index--)
            {
                var score = new int[1 + 1 + index];
                var groupSet = new SortedSet<int[]>(this);
                var group = int.MaxValue;
                foreach (var info in _contextDatabases[index].SortedEnumerable)
                {
                    var token = info.Token;

                    if (IsNewToken(token))
                    {
                        score[0] = token;
                        score[1 + index] = info.Count;

                        for (var inbetweenIndex = 0; inbetweenIndex < index; inbetweenIndex++)
                        {
                            if (_contextDatabases[inbetweenIndex].TryGetValue(token, out var inbetweenInfo))
                            {
                                score[1 + inbetweenIndex] = inbetweenInfo.Count;
                            }
                            else
                            {
                                Debug.Fail("Cannot not find an inbetween");
                                score[1 + inbetweenIndex] = 0;
                            }
                        }

                        if (group != info.Count)
                        {
                            foreach (var s in groupSet)
                            {
                                yield return s;
                            }
                            groupSet.Clear();

                            Debug.Assert(info.Count < group);
                            group = info.Count;
                        }

                        groupSet.Add(score);
                        score = new int[1 + 1 + index];
                    }
                }

                foreach (var s in groupSet)
                {
                    yield return s;
                }
            }

            // Produce scores with two ordinls - those guaranteed to be produced in order.
            {
                var score = new int[1 + 1];
                foreach (var info in _contextDatabases[0].SortedEnumerable)
                {
                    var token = info.Token;

                    if (IsNewToken(token))
                    {
                        score[0] = token;
                        score[1] = info.Count;

                        yield return score;
                    }
                }
            }

            // As a first fallback produce single ordinal results.
            var singleToken = new int[1];
            var tokens = _source.GetTokens();

            using (var enumerator = tokens.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var token = enumerator.Current;

                    if (IsNewToken(token))
                    {
                        singleToken[0] = token;
                        yield return singleToken;
                    }
                }
            }

            // TODO: We should now perhaps disregard index position constraints and just yield anything.
        }

        internal static IEnumerable<int[]> GetTopScores(PredictiveVocabularySource<T> source,
            TokenPredictorDatabase database,
            ITokenTileFilter filter,
            int[] context,
            int contextWidth,
            int minIndex,
            int limIndex)
        {
            var maker = new ScoredTokenPredictionMaker<T>(source, database, filter, context, minIndex, limIndex);
            var scores = maker.GetTopScores(filter == null);
            return scores;
        }

        int IComparer<int[]>.Compare(int[] x, int[] y)
        {
            Debug.Assert(x.Length == y.Length);
            var position = x.Length - 1;
            while (x[position] == y[position])
            {
                position--;
            }
            var value = x[position] < y[position] ? +1 : -1;
            return value;
        }
    }
}
