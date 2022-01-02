using System.Collections.Generic;
using System.Text;

namespace TreeSortish
{
    internal abstract class Item
    {
        internal Item(RealItem parent, Node node, int count)
        {
            Parent = parent;
            Node = node;
            Count = count;
        }

        internal virtual RealItem Real => null;

        internal virtual PotentialItem Potential => null;

        internal RealItem Parent { get; }

        internal Node Node { get; }

        internal int Count { get; }
    }

    internal class RealItem : Item
    {
        internal RealItem(RealItem parent, Node node)
            : base(parent, node, node.Count)
        {
        }

        internal RealItem(RealItem parent, IEnumerator<Node> enumerator)
            : this(parent, enumerator.Current)
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
            AppendAncestors(builder, Parent);
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
            : base(parent, node, count)
        {
        }

        internal PotentialItem(RealItem item)
            : base(item.Parent, item.Node, item.Count - 1)
        {
        }

        internal override PotentialItem Potential => this;
    }
}
