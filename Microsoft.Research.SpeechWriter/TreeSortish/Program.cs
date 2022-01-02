﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var expected = new[]
            {
                "25: aardvarks ... are nice",
                "24: hello ... mr charles esq",
                "8: hello mr brian ... esq",
                "7: zebras ... are stripy",
                "6: hello mr adam ... esq"
            };

            var position = 0;
            foreach (var item in walk)
            {
                var text = item.ToString();
                Console.WriteLine(text);

                Debug.Assert(text == expected[position], $"Expected {expected[position]}");
                position++;
            }
            Debug.Assert(position == expected.Length);
        }

        private static IEnumerable<Item> FindOrderedItems(List<Node> database)
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

                    if (item.IsReal)
                    {
                        yield return item;

                        if (item.Enumerator.MoveNext())
                        {
                            var newReal = new Item(item.Parent, item.Enumerator);
                            AddSecondary(queue, newReal);
                        }
                        else
                        {
                            item.Enumerator.Dispose();
                        }

                        if (1 < item.Count)
                        {
                            item.MakePotential();
                            AddSecondary(queue, item);
                        }
                    }
                    else
                    {
                        ExpandPotentialItem(queue, item);
                    }
                }
            }
        }

        private static void ExpandPotentialItem(List<Item> queue, Item potentialItem)
        {
            var item = potentialItem;

            var done = false;
            while (!done)
            {
                var parent = item.Parent;
                var node = item.Node;

                var enumerator = node.Children.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    var firstNode = enumerator.Current;

                    if (enumerator.MoveNext())
                    {
                        var newReal = new Item(item, enumerator);

                        AddSecondary(queue, newReal);
                        done = true;
                    }
                    else
                    {
                        // Walking down single branch.
                        enumerator.Dispose();

                        var newReal = new Item(item, firstNode);

                        item = newReal;
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
                    Sort(node.ChildList);
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

        private static void DumpDatabase(string prefix, IEnumerable<Node> database)
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

                Add(node.ChildList, words, index + 1, count);
            }
        }
    }
}