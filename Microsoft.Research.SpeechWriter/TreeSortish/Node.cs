using System.Collections.Generic;

namespace TreeSortish
{
    internal class Node : IPredictableNode<Node>
    {
        internal string Word { get; set; }

        public int Count { get; set; }

        internal List<Node> ChildList { get; } = new List<Node>();

        internal IEnumerable<Node> Children => ChildList;

        public IEnumerable<Node> GetChildren() => ChildList;

        public override string ToString() => $"{Word}*{Count}";
    }
}
