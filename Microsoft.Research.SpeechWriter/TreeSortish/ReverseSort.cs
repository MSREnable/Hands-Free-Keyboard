using System.Collections.Generic;

namespace TreeSortish
{
    internal class ReverseSort : IComparer<int>
    {
        internal static ReverseSort Instance = new ReverseSort();

        int IComparer<int>.Compare(int x, int y)
        {
            var value = y.CompareTo(x);
            return value;
        }
    }
}
