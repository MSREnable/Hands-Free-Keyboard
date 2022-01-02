using System.Collections.Generic;

namespace TreeSortish
{
    internal interface IPredictableNode<T>
        where T : IPredictableNode<T>
    {
        int Count { get; }

        IEnumerable<T> GetChildren();
    }
}
