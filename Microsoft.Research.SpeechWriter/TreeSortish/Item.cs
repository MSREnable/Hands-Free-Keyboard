using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TreeSortish
{
    internal sealed class Item
    {
        internal Item(Item parent, Node node)
        {
            Parent = parent;
            Node = node;
            Count = node.Count;
            IsReal = true;
        }

        internal Item(Item parent, IEnumerator<Node> enumerator)
            : this(parent, enumerator.Current)
        {
            Enumerator = enumerator;
        }

        internal Item Parent { get; }

        internal Node Node { get; }

        internal int Count { get; private set; }

        internal IEnumerator<Node> Enumerator { get; }

        internal bool IsReal { get; private set; }

        internal void MakePotential()
        {
            Debug.Assert(IsReal);

            IsReal = false;
            Count--;
        }

        public override string ToString()
        {
            var builder = new StringBuilder($"{Count}:");

            void AppendAncestors(Item item)
            {
                if (item != null)
                {
                    AppendAncestors(item.Parent);

                    builder.Append($" {item.Node.Word}");
                }
            }

            AppendAncestors(Parent);
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
}
