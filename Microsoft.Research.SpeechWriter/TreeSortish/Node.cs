using System.Collections.Generic;

namespace TreeSortish
{
    internal class Node
    {
        internal string Word { get; set; }

        internal int Count { get; set; }

        internal List<Node> ChildList { get; } = new List<Node>();

        internal IEnumerable<Node> Children => ChildList;

        public override string ToString() => $"{Word}*{Count}";
    }
}
