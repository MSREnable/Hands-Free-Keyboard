using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TreeSortish
{
    internal abstract class Item
    {
        internal Item(Node[] path, List<Node> container, int index, int count)
        {
            Path = path;
            Container = container;
            Index = index;
            Count = count;

            Debug.Assert(path.Length == 0 || !ReferenceEquals(path[path.Length - 1], Container[index]));
        }

        internal virtual RealItem Real => null;

        internal virtual PotentialItem Potential => null;

        internal Node[] Path { get; }

        internal List<Node> Container { get; }

        internal int Index { get; }

        internal int Count { get; }
    }

    internal class RealItem : Item
    {
        internal RealItem(Node[] path, IEnumerator<Node> enumerator, List<Node> container, int index, int count)
            : base(path, container, index, count)
        {
            Enumerator = enumerator;
            Tail = enumerator.Current;

            Debug.Assert(ReferenceEquals(Tail, Container[Index]));
        }

        internal override RealItem Real => this;

        internal IEnumerator<Node> Enumerator { get; }

        internal Node Tail { get; }

        internal void Dump()
        {
            var text = $"{Count}:";
            foreach (var node in Path)
            {
                text += $" {node.Word}";
            }
            text += $" {Tail.Word}";

            text += " ...";

            var lastNode = Tail;
            while (lastNode.Children.Count != 0)
            {
                lastNode = lastNode.Children[0];
                text += $" {lastNode.Word}";
            }

            Console.WriteLine(text);
        }
    }

    internal class PotentialItem : Item
    {
        internal PotentialItem(Node[] path, Node tail, List<Node> container, int index, int count)
            : base(path, container, index, count)
        {
            Debug.Assert(ReferenceEquals(tail, Container[Index]));
            Tail = container[index];
        }

        internal override PotentialItem Potential => this;

        internal Node Tail { get; }
    }
}
