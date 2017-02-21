using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.HandsFree.Prediction.Historic
{
    /// <summary>
    /// Class for identifying depth slices through a depth map.
    /// </summary>
    public class Plumber<T>
    {
        readonly T _reference;
        readonly int _limit;
        readonly Func<T, int, int> _getDepth;
        readonly Func<T, int, int> _comparer;

        int _maxDepthPosition;

        Plumber(T reference, Func<T, int, int> getDepth, Func<T, int, int> comparer, int limit)
        {
            _reference = reference;
            _getDepth = getDepth;
            _comparer = comparer;
            _limit = limit;
        }

        /// <summary>
        /// The maximum depth in the depth map.
        /// </summary>
        public int MaxDepth { get { return (int)_getDepth(_reference, _maxDepthPosition); } }

        void FindMaximumDepth()
        {
            var start = 1;
            var limit = _limit;

            Debug.Assert(0 <= _comparer(_reference, 0));

            while (start < limit)
            {
                var split = (start + limit) / 2;

                if (_comparer(_reference, split) < 0)
                {
                    limit = split;
                }
                else
                {
                    start = split + 1;
                }
            }

            _maxDepthPosition = start - 1;

            // The optimal position has been found, but it is possible where a partial match
            // has been foung, the adjacent position is actually the deepest match!
            var maxDepth = _getDepth(_reference, _maxDepthPosition);
            if (start < _limit && maxDepth == 0 && _getDepth(_reference, start) != 0)
            {
                _maxDepthPosition++;

                Debug.Assert(_maxDepthPosition == start);
            }
        }

        /// <summary>
        /// Create a plumber object from a depth map.
        /// </summary>
        /// <param name="reference">Reference value for plumbing.</param>
        /// <param name="getDepth">Function for obtaining the depth at a given index.</param>
        /// <param name="comparer">Function for comparing relative position of reference value to stored value.</param>
        /// <param name="limit">Number of items in the depth map.</param>
        /// <returns>The plumbing object.</returns>
        public static Plumber<T> Create(T reference, Func<T, int, int> getDepth, Func<T, int, int> comparer, int limit)
        {
            var plumber = new Plumber<T>(reference, getDepth, comparer, limit);

            plumber.FindMaximumDepth();

            return plumber;
        }

        /// <summary>
        /// Find the insert position for a reference value.
        /// </summary>
        /// <param name="reference">The reference value.</param>
        /// <param name="comparer">Function for testing relative position of item to maximum depth item.</param>
        /// <param name="initiallLimit">Number of items in the depth map.</param>
        /// <returns></returns>
        public static int FindInsertPosition(T reference, Func<T, int, int> comparer, int initiallLimit)
        {
            var start = 0;
            var limit = initiallLimit;

            while (start < limit)
            {
                var split = (start + limit) / 2;

                if (comparer(reference, split) < 0)
                {
                    limit = split;
                }
                else
                {
                    start = split + 1;
                }
            }

            return start;
        }

        int FindDepthStart(int depth, int initialLimit)
        {
            var start = 0;
            var limit = initialLimit;

            while (start < limit)
            {
                var split = (start + limit) / 2;

                if (depth <= _getDepth(_reference, split))
                {
                    limit = split;
                }
                else
                {
                    start = split + 1;
                }
            }

            return start;
        }

        int FindDepthLimit(int depth, int initialStart)
        {
            var start = initialStart;
            var limit = _limit;

            while (start < limit)
            {
                var split = (start + limit) / 2;

                if (depth <= _getDepth(_reference, split))
                {
                    start = split + 1;
                }
                else
                {
                    limit = split;
                }
            }

            return limit;
        }

        /// <summary>
        /// Get the depth slice for a given depth.
        /// </summary>
        /// <param name="depth">The depth to plumb for.</param>
        /// <returns>The slice for that depth.</returns>
        public DepthSlice GetDepthSlice(int depth)
        {
            var start = FindDepthStart(depth, _maxDepthPosition);
            var limit = FindDepthLimit(depth, _maxDepthPosition + 1);
            var slice = new DepthSlice(start, limit);
            return slice;
        }

        /// <summary>
        /// Obtain enumerator for all the different depth slices through the depth map from the greatest to the least.
        /// </summary>
        /// <returns>An emumerator yielding depth slices.</returns>
        public IEnumerable<DepthSlice> GetDepthSlices()
        {
            var depth = MaxDepth;
            var start = FindDepthStart(depth, _maxDepthPosition);
            var limit = FindDepthLimit(depth, _maxDepthPosition + 1);

            if (limit != start + 1 && _comparer(_reference, start) == 0)
            {
                yield return new DepthSlice(start, start + 1);
            }

            yield return new DepthSlice(start, limit);

            while (start != 0 || limit != _limit)
            {
                if (start == 0)
                {
                    Debug.Assert(limit < _limit);
                    depth = _getDepth(_reference, limit);
                }
                else if (limit == _limit)
                {
                    Debug.Assert(0 < start);
                    depth = _getDepth(_reference, start - 1);
                }
                else
                {
                    depth = Math.Max(_getDepth(_reference, start - 1), _getDepth(_reference, limit));
                }

                start = FindDepthStart(depth, start);
                limit = FindDepthLimit(depth, limit);

                yield return new DepthSlice(start, limit);
            }
        }
    }
}
