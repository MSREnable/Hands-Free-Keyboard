using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class TokenPredictorDatabase
    {
        private readonly Dictionary<int, TokenPredictorInfo> _database = new Dictionary<int, TokenPredictorInfo>();

        public IEnumerable<int> Keys => _database.Keys;

        internal TokenPredictorInfo GetValue(int token)
        {
            if (!_database.TryGetValue(token, out var info))
            {
                info = new TokenPredictorInfo();
                _database.Add(token, info);
            }

            return info;
        }

        internal bool TryGetValue(int token, out TokenPredictorInfo info)
        {
            var value = _database.TryGetValue(token, out info);
            return value;
        }

        internal void Clear()
        {
            _database.Clear();
        }

        public Dictionary<int, TokenPredictorInfo>.Enumerator GetEnumerator()
        {
            return _database.GetEnumerator();
        }

        internal void Remove(int token)
        {
            _database.Remove(token);
        }

        internal IReadOnlyList<int> GetTopRanked()
        {
            var maxCount = 0;
            var tokens = new List<int>();

            foreach (var pair in _database)
            {
                var count = pair.Value.Count;

                if (maxCount <= count)
                {
                    if (maxCount < count)
                    {
                        maxCount = count;
                        tokens.Clear();
                    }

                    tokens.Add(pair.Key);
                }
            }

            return tokens;
        }

        internal TokenPredictorDatabase GetChild(int[] context, int index, int length)
        {
            Debug.Assert(1 <= length);

            TokenPredictorDatabase result;

            if (_database.TryGetValue(context[index], out var info))
            {
                if (length == 1)
                {
                    result = info.TryGetChildren();
                }
                else
                {
                    result = info.GetChild(context, index + 1, length - 1);
                }
            }
            else
            {
                result = null;
            }

            return result;
        }
    }
}
