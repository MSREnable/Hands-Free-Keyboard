using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeSortish
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var database = CreateDatabase();

            // DumpDatabase("Unsorted: ", database);

            Sort(database);

            // DumpDatabase("Sorted: ", database);

            Walk(database);

            Console.ReadKey();
        }

        private static void Walk(List<Node> database)
        {
            var walk = FindOrderedItems(database);
            foreach (var item in walk)
            {
                item.Dump();
            }
        }

        private static IEnumerable<Item> FindOrderedItems(List<Node> database)
        {
            if (database.Count != 0)
            {
                var seedItem = new Item(new Node[0], database, 0, database[0].Count, true);
                var queue = new List<Item> { seedItem };

                while (queue.Count != 0)
                {
                    var item = queue[0];
                    queue.RemoveAt(0);

                    if (item.IsReal)
                    {
                        yield return item;

                        var index = item.Container.IndexOf(item.Tail);
                        var nextIndex = index + 1;
                        if (nextIndex < item.Container.Count)
                        {
                            var nextItem = item.Container[nextIndex];
                            var newReal = new Item(item.Path, item.Container, nextIndex, nextItem.Count, true);
                            AddSecondary(queue, newReal);
                        }

                        if (1 < item.Count)
                        {
                            var newFake = new Item(item.Path, item.Container, item.Index, item.Count - 1, false);
                            AddSecondary(queue, newFake);
                        }
                    }
                    else
                    {
                        var newPath = new List<Node>(item.Path);
                        var node = item.Tail;
                        if (node.Children.Count == 1)
                        {
                            newPath.Add(node);
                            node = node.Children[0];
                        }

                        if (1 < node.Children.Count)
                        {
                            newPath.Add(node);
                            var newReal = new Item(newPath.ToArray(), node.Children, 1, node.Count, true);
                            AddSecondary(queue, newReal);
                        }
                    }
                }
            }
        }

        private static void AddSecondary(List<Item> secondarySource, Item newSecondary)
        {
            var position = 0;
            while (position < secondarySource.Count && newSecondary.Count <= secondarySource[position].Count)
            {
                position++;
            }
            secondarySource.Insert(position, newSecondary);
        }

        private static void Sort(List<Node> database)
        {
            if (database.Count != 0)
            {
                database.Sort((x, y) => y.Count.CompareTo(x.Count));

                foreach (var node in database)
                {
                    Sort(node.Children);
                }
            }
        }

        private static List<Node> CreateDatabase()
        {
            var database = new List<Node>();

            Add(database, "aardvarks are nice", 25);
            Add(database, "zebras are stripy", 7);
            Add(database, "hello mr adam esq", 6);
            Add(database, "hello mr brian esq", 8);
            Add(database, "hello mr charles esq", 10);
            return database;
        }

        private static void DumpDatabase(string prefix, List<Node> database)
        {
            using (var enumerator = database.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    var first = $"{prefix}{enumerator.Current.Word}*{enumerator.Current.Count} ";
                    DumpDatabase(first, enumerator.Current.Children);

                    var leadIn = new string(' ', prefix.Length);
                    while (enumerator.MoveNext())
                    {
                        var subsequent = $"{leadIn}{enumerator.Current.Word}*{enumerator.Current.Count} ";
                        DumpDatabase(subsequent, enumerator.Current.Children);
                    }
                }
                else
                {
                    Console.WriteLine(prefix);
                }
            }
        }

        private static void Add(List<Node> database, string sentence, int count)
        {
            var words = sentence.Split(' ');
            Add(database, words, 0, count);
        }

        private static void Add(List<Node> database, string[] words, int index, int count)
        {
            if (index < words.Length)
            {
                var node = database.FirstOrDefault(n => n.Word == words[index]);
                if (node != null)
                {
                    node.Count += count;
                }
                else
                {
                    node = new Node { Word = words[index], Count = count };
                    database.Add(node);
                }

                Add(node.Children, words, index + 1, count);
            }
        }
    }
}