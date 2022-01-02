using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TreeSortish
{
    internal abstract class Item
    {
        internal Item(RealItem parent, Node[] path, Node node, int count)
        {
            Parent = parent;
            Path = path;
            Node = node;
            Count = count;

            if (parent == null)
            {
                Debug.Assert(path.Length == 0);
            }
            else
            {
                Debug.Assert(path.Length != 0 && ReferenceEquals(parent.Node, path[path.Length - 1]));
            }
        }

        internal virtual RealItem Real => null;

        internal virtual PotentialItem Potential => null;

        internal RealItem Parent { get; }

        internal Node[] Path { get; }

        internal Node Node { get; }

        internal int Count { get; }
    }

    internal class RealItem : Item
    {
        internal RealItem(RealItem parent, Node[] path, Node node)
            : base(parent, path, node, node.Count)
        {
            var ancestorCount = 0;
            for (var ancestorItem = parent; ancestorItem != null; ancestorItem = ancestorItem.Parent)
            {
                ancestorCount++;
            }

            Debug.Assert(ancestorCount == path.Length);

            var ancestor = parent;
            var expectedLimit = path.Length;

            while (ancestor != null && expectedLimit != 0)
            {
                var expected = path[expectedLimit - 1];
                var actual = ancestor.Node;
                //Debug.Assert(ReferenceEquals(expected, actual));
                ancestor = ancestor.Parent;
                expectedLimit--;
            }
            Debug.Assert(ancestor == null && expectedLimit == 0);
        }

        internal RealItem(RealItem parent, Node[] path, IEnumerator<Node> enumerator)
            : this(parent, path, enumerator.Current)
        {
            Enumerator = enumerator;
        }

        internal override RealItem Real => this;

        internal IEnumerator<Node> Enumerator { get; }

        private static void AppendAncestors(StringBuilder builder, RealItem item)
        {
            if (item != null)
            {
                AppendAncestors(builder, item.Parent);

                builder.Append($" {item.Node.Word}");
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder($"{Count}:");
            //AppendAncestors(builder, Parent);
            foreach (var node in Path)
            {
                builder.Append($" {node.Word}");
            }
            builder.Append($" {Node.Word}");

            builder.Append(" ...");

            var lastNode = Node;
            while (lastNode != null)
            {
                using (var enumerator = lastNode.Children.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        lastNode = enumerator.Current;
                        builder.Append($" {lastNode.Word}");
                    }
                    else
                    {
                        lastNode = null;
                    }
                }
            }

            var text = builder.ToString();
            return text;
        }
    }

    internal class PotentialItem : Item
    {
        internal PotentialItem(RealItem parent, Node[] path, Node node, int count)
            : base(parent, path, node, count)
        {
        }

        internal PotentialItem(RealItem item)
            : base(item.Parent, item.Path, item.Node, item.Count - 1)
        {
        }

        internal override PotentialItem Potential => this;
    }
}
