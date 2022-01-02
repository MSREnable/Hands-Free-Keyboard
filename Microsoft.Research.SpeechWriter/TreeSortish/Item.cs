using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TreeSortish
{
    internal sealed class Item
    {
        private readonly Item _parent;

        private readonly Node _node;

        private int _count;

        private readonly IEnumerator<Node> _enumerator;

        private bool _isReal;

        internal Item(Item parent, Node node)
        {
            _parent = parent;
            _node = node;
            _count = node.Count;
            _isReal = true;
        }

        internal Item(Item parent, IEnumerator<Node> enumerator)
            : this(parent, enumerator.Current)
        {
            _enumerator = enumerator;
        }

        internal static IEnumerable<Item> FindOrderedItems(IEnumerable<Node> database)
        {
            var seedEnumerator = database.GetEnumerator();
            if (seedEnumerator.MoveNext())
            {
                var seedItem = new Item(null, seedEnumerator);
                var queue = new List<Item> { seedItem };

                while (queue.Count != 0)
                {
                    var item = queue[0];
                    queue.RemoveAt(0);

                    if (item._isReal)
                    {
                        yield return item;

                        if (item._enumerator.MoveNext())
                        {
                            var nextItem = new Item(item._parent, item._enumerator);
                            nextItem.Enqueue(queue);
                        }
                        else
                        {
                            item._enumerator.Dispose();
                        }

                        if (1 < item._count)
                        {
                            item.MakePotential();
                            item.Enqueue(queue);
                        }
                    }
                    else
                    {
                        item.ExpandPotentialParent(queue);
                    }
                }
            }
        }

        private void ExpandPotentialParent(List<Item> queue)
        {
            var item = this;

            var done = false;
            while (!done)
            {
                var node = item._node;
                var enumerator = node.Children.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    var firstNode = enumerator.Current;

                    if (enumerator.MoveNext())
                    {
                        item = new Item(item, enumerator);

                        item.Enqueue(queue);
                        done = true;
                    }
                    else
                    {
                        // Walking down single branch.
                        enumerator.Dispose();

                        item = new Item(item, firstNode);
                    }
                }
                else
                {
                    // No potential next node.
                    enumerator.Dispose();
                    done = true;
                }
            }
        }

        private void Enqueue(List<Item> queue)
        {
            var position = 0;
            while (position < queue.Count && _count <= queue[position]._count)
            {
                position++;
            }
            queue.Insert(position, this);
        }

        private void MakePotential()
        {
            Debug.Assert(_isReal);

            _isReal = false;
            _count--;
        }

        public override string ToString()
        {
            var builder = new StringBuilder($"{_count}:");

            void AppendAncestors(Item item)
            {
                if (item != null)
                {
                    AppendAncestors(item._parent);

                    builder.Append($" {item._node.Word}");
                }
            }

            AppendAncestors(_parent);
            builder.Append($" {_node.Word}");

            builder.Append(" ...");

            var lastNode = _node;
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
