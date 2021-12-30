using System;
using System.Collections.Generic;

namespace TreeSortish
{
    internal abstract class Item
    {
        internal Item(Node[] path, int count)
        {
            Path = path;
            Count = count;
        }

        internal virtual RealItem Real => null;

        internal virtual PotentialItem Potential => null;

        internal Node[] Path { get; }

        internal int Count { get; }
    }

    internal class RealItem : Item
    {
        internal RealItem(Node[] path, IEnumerator<Node> enumerator)
            : base(path, enumerator.Current.Count)
        {
            Enumerator = enumerator;
            Tail = enumerator.Current;
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
            while (lastNode != null)
            {
                using (var enumerator = lastNode.Children.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        lastNode = enumerator.Current;
                        text += $" {lastNode.Word}";
                    }
                    else
                    {
                        lastNode = null;
                    }
                }
            }

            Console.WriteLine(text);
        }
    }

    internal class PotentialItem : Item
    {
        internal PotentialItem(Node[] path, Node tail, int count)
            : base(path, count)
        {
            Tail = tail;
        }

        internal override PotentialItem Potential => this;

        internal Node Tail { get; }
    }
}
