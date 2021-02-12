using System.Collections.Generic;

namespace Microsoft.Research.RankWriter.Library
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
    }
}
