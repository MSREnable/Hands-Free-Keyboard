using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TreeSortish
{
    internal class Item
    {
        internal Item(Node[] path, List<Node> container, int index, int count, bool isReal)
        {
            Container = container;
            Path = path;
            Index = index;
            Count = count;
            IsReal = isReal;

            Debug.Assert(path.Length == 0 || !ReferenceEquals(path[path.Length - 1], Container[index]));
        }

        internal List<Node> Container { get; }

        internal Node[] Path { get; }

        internal Node Tail => Container[Index];

        internal int Index { get; }

        internal int Count { get; }

        internal bool IsReal { get; }

        internal void Dump()
        {
            var text = $"{Count}:";
            foreach (var node in Path)
            {
                text += $" {node.Word}";
            }
            text += $" {Tail.Word}";

            text += " ...";

            var lastNode = Container[Index];
            while (lastNode.Children.Count != 0)
            {
                lastNode = lastNode.Children[0];
                text += $" {lastNode.Word}";
            }

            Console.WriteLine(text);
        }
    }
}
