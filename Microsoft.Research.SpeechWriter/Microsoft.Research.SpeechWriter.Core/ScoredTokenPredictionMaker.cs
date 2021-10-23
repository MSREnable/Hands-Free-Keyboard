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
        private readonly int[] _context;
        private readonly int _minIndex;
        private readonly int _limIndex;

        /// <summary>
        /// Tokens already found and returned.
        /// </summary>
        private readonly HashSet<int> _foundTokens = new HashSet<int>();

        internal ScoredTokenPredictionMaker(PredictiveVocabularySource<T> source, TokenPredictorDatabase database, ITokenTileFilter filter, int[] context, int contextWidth, int minIndex, int limIndex)
        {
            _source = source;
            _database = database;
            _filter = filter;

            var contextLimit = context.Length;
            var contextStart = Math.Max(0, contextLimit - contextWidth + 1);
            var contextLength = contextLimit - contextStart;
            _context = new int[contextLength];
            Array.Copy(context, contextStart, _context, 0, contextLength);

            _context = context;
            _minIndex = minIndex;
            _limIndex = limIndex;
        }

        private bool IsNewToken(int token)
        {
            bool value;

            if (!_foundTokens.Contains(token))
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
            else
            {
                value = false;
            }

            return value;
        }

        private TokenPredictorDatabase GetContextDatabase(int contextStart)
        {
            var contextLength = _context.Length;
            var database = _database;

            for (var index = contextStart; database != null && index < contextLength; index++)
            {
                if (database.TryGetValue(_context[index], out var info))
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

        internal TokenPredictorDatabase[] GetContextDatabases()
        {
            var contextLength = _context.Length;
            var databases = new List<TokenPredictorDatabase>(contextLength + 1) { _database };

            var done = false;
            for (var index = 1; index <= contextLength && !done; index++)
            {
                var database = GetContextDatabase(contextLength - index);
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

        internal IEnumerable<int[]> GetTopScores()
        {
            // Get the databases that represent increasing depths into the context.
            var databases = GetContextDatabases();

            // Produce scores with three or more elements - those that may not entirely be ordered
            // by their final ordinal.
            for (var index = databases.Length - 1; 1 <= index; index--)
            {
                var score = new int[1 + 1 + index];
                var groupSet = new SortedSet<int[]>(this);
                var group = int.MaxValue;
                foreach (var info in databases[index].SortedEnumerable)
                {
                    var token = info.Token;

                    if (IsNewToken(token))
                    {
                        score[0] = token;
                        score[1 + index] = info.Count;

                        for (var inbetweenIndex = 0; inbetweenIndex < index; inbetweenIndex++)
                        {
                            if (databases[inbetweenIndex].TryGetValue(token, out var inbetweenInfo))
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
                foreach (var info in databases[0].SortedEnumerable)
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
            var maker = new ScoredTokenPredictionMaker<T>(source, database, filter, context, contextWidth, minIndex, limIndex);
            var scores = maker.GetTopScores();
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
