using System.Collections.Generic;
using System.Diagnostics;

namespace TreeSortish
{
    internal sealed class Item<TNode> : IEnumerable<TNode>
        where TNode : class, IPredictableNode<TNode>
    {
        private readonly Item<TNode> _parent;

        private readonly TNode _node;

        private int _count;

        private readonly IEnumerator<TNode> _enumerator;

        private bool _isReal;

        private Item(Item<TNode> parent, TNode node)
        {
            _parent = parent;
            _node = node;
            _count = node.Count;
            _isReal = true;
        }

        internal Item(Item<TNode> parent, IEnumerator<TNode> enumerator)
            : this(parent, enumerator.Current)
        {
            _enumerator = enumerator;
        }

        internal static IEnumerable<IEnumerable<TNode>> FindOrderedItems(IEnumerable<TNode> database)
        {
            var seedEnumerator = database.GetEnumerator();
            if (seedEnumerator.MoveNext())
            {
                var seedItem = new Item<TNode>(null, seedEnumerator);
                var queue = new List<Item<TNode>> { seedItem };

                while (queue.Count != 0)
                {
                    var item = queue[0];
                    queue.RemoveAt(0);

                    if (item._isReal)
                    {
                        yield return item;

                        if (item._enumerator.MoveNext())
                        {
                            var nextItem = new Item<TNode>(item._parent, item._enumerator);
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

        private void ExpandPotentialParent(List<Item<TNode>> queue)
        {
            var item = this;

            var done = false;
            while (!done)
            {
                var node = item._node;
                var enumerator = node.GetChildren().GetEnumerator();

                if (enumerator.MoveNext())
                {
                    var firstNode = enumerator.Current;

                    if (enumerator.MoveNext())
                    {
                        item = new Item<TNode>(item, enumerator);

                        item.Enqueue(queue);
                        done = true;
                    }
                    else
                    {
                        // Walking down single branch.
                        enumerator.Dispose();

                        item = new Item<TNode>(item, firstNode);
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

        private void Enqueue(List<Item<TNode>> queue)
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

        public IEnumerator<TNode> GetEnumerator()
        {
            var list = new List<TNode>();

            var ancestor = this;
            do
            {
                list.Insert(0, ancestor._node);
                ancestor = ancestor._parent;
            }
            while (ancestor != null);

            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
