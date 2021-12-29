using System.Collections.Generic;

namespace TreeSortish
{
    internal class Node
    {
        internal string Word { get; set; }

        internal int Count { get; set; }

        internal List<Node> Children { get; } = new List<Node>();

        public override string ToString() => $"{Word}*{Count}";
    }
}
