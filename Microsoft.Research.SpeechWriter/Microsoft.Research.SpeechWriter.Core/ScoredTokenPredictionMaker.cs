﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    class ScoredTokenPredictionMaker : IComparer<int[]>
    {
        private readonly PredictiveVocabularySource _source;
        private readonly Func<int, bool> _tokenFilter;
        private readonly TokenPredictorDatabase[] _contextDatabases;

        private ScoredTokenPredictionMaker(PredictiveVocabularySource source, Func<int, bool> tokenFilter, TokenPredictorDatabase[] contextDatabases)
        {
            _source = source;
            _contextDatabases = contextDatabases;
            _tokenFilter = tokenFilter;
        }

        internal ScoredTokenPredictionMaker(PredictiveVocabularySource source, TokenPredictorDatabase database, Func<int, bool> tokenFilter, int[] context)
            : this(source, tokenFilter, GetContextDatabases(database, context))
        {
        }

        private static TokenPredictorDatabase GetContextDatabase(TokenPredictorDatabase rootDatabase, int[] context, int contextStart)
        {
            var contextLength = context.Length;
            var database = rootDatabase;

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

        private static TokenPredictorDatabase[] GetContextDatabases(TokenPredictorDatabase rootDatabase, int[] context)
        {
            var contextLength = context.Length;
            var databases = new List<TokenPredictorDatabase>(contextLength + 1) { rootDatabase };

            var done = false;
            for (var index = 1; index <= contextLength && !done; index++)
            {
                var database = GetContextDatabase(rootDatabase, context, contextLength - index);
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

        internal static TokenPredictorDatabase[] GetNextContextDatabases(TokenPredictorDatabase[] previous, int token)
        {
            var previousLength = previous.Length;
            var databases = new List<TokenPredictorDatabase>(previousLength + 1) { previous[0] };

            var done = false;
            for (var index = 0; index < previousLength && !done; index++)
            {
                if (previous[index].TryGetValue(token, out var info))
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

        internal ScoredTokenPredictionMaker CreateNextPredictionMaker(int token, Func<int, bool> tokenFilter)
        {
            var databases = GetNextContextDatabases(_contextDatabases, token);
            var maker = new ScoredTokenPredictionMaker(_source, tokenFilter, databases);
            return maker;
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

        internal IEnumerable<int[]> GetTopScores(int minIndex, int limIndex, bool unfiltered, bool includeSpeculative)
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
                    if (minIndex <= index && index < limIndex)
                    {
                        _foundTokens.Add(token);

                        if (_tokenFilter(token))
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

            if (includeSpeculative)
            {
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
