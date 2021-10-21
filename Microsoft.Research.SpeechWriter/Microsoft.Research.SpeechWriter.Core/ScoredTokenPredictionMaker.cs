using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    class ScoredTokenPredictionMaker<T>
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

        internal IEnumerable<int[]> GetTopScores()
        {
            var contextLength = _context.Length;
            var databases = new TokenPredictorDatabase[contextLength + 1];
            databases[0] = _database;
            for (var index = 1; index <= contextLength && databases[index - 1] != null; index++)
            {
                databases[index] = GetContextDatabase(contextLength - index);
            }

            for (var index = contextLength; 0 <= index; index--)
            {
                if (databases[index] != null)
                {
                    var score = new int[1 + 1 + index];
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

                            yield return score;
                        }
                    }
                }
            }

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
    }
}
