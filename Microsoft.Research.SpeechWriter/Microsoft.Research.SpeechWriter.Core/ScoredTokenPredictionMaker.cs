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

        private IEnumerable<int[]> GetChildScores(int contextStart)
        {
            var pathLength = _context.Length - contextStart;
            var score = new int[1 + 1 + pathLength];

            var database = _database;
            for (var index = 0; database != null && index < pathLength; index++)
            {
                if (database.TryGetValue(_context[contextStart + index], out var info))
                {
                    score[1 + index] = info.Count;
                    database = info.TryGetChildren();
                }
            }

            if (database != null)
            {
                var previousCount = int.MaxValue;
                var previousToken = int.MaxValue;

                foreach (var info in database.SortedEnumerable)
                {
                    if (IsNewToken(info.Token))
                    {
                        score[0] = info.Token;
                        score[1 + pathLength] = info.Count;

                        Debug.Assert(info.Count <= previousCount);
                        Debug.Assert(info.Count < previousCount || info.Token < previousToken);
                        previousCount = info.Count;
                        previousToken = info.Token;

                        yield return score;
                    }
                }
            }
        }

        private IEnumerable<int[]> GetTokenScores()
        {
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

        internal IEnumerable<int[]> GetTopScores()
        {
            var contextLength = _context.Length;
            for (var scanIndex = 0; scanIndex <= contextLength; scanIndex++)
            {
                var childScores = GetChildScores(scanIndex);
                foreach (var score in childScores)
                {
                    yield return score;
                }
            }

            var tokenScores = GetTokenScores();
            foreach (var score in tokenScores)
            {
                yield return score;
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
