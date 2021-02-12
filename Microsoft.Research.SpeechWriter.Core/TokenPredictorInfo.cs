using System.Diagnostics;

namespace Microsoft.Research.RankWriter.Library
{
    internal class TokenPredictorInfo
    {
        private int _count;

        private TokenPredictorDatabase _children;

        internal int Count => _count;

        internal void IncrementCount(int increment)
        {
            Debug.Assert(0 < increment);

            _count += increment;
        }

        internal int DecrementCount(int decrement)
        {
            Debug.Assert(decrement <= _count);

            _count -= decrement;

            return _count;
        }

        internal TokenPredictorDatabase GetChildren()
        {
            if (_children == null)
            {
                _children = new TokenPredictorDatabase();
            }

            return _children;
        }

        internal TokenPredictorInfo GetChildInfo(int token)
        {
            var children = GetChildren();

            var info = _children.GetValue(token);

            return info;
        }

        internal TokenPredictorDatabase TryGetChildren()
        {
            return _children;
        }

    }
}
