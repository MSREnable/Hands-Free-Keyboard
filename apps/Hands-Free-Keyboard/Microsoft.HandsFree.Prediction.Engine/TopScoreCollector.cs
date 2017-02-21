namespace Microsoft.HandsFree.Prediction.Engine
{
    using System.Collections.Generic;
    using System.Linq;

    class TopScoreCollector
    {
        readonly int count;

        readonly List<KeyValuePair<string, double>> list = new List<KeyValuePair<string, double>>();

        internal TopScoreCollector(int count)
        {
            this.count = count;
        }

        static bool IsOrdered(KeyValuePair<string, double> lhs, KeyValuePair<string, double> rhs)
        {
            bool isOrdered;

            if (lhs.Value > rhs.Value)
            {
                isOrdered = true;
            }
            else if (lhs.Value == rhs.Value)
            {
                if (lhs.Key.CompareTo(rhs.Key) < 0)
                {
                    isOrdered = true;
                }
                else
                {
                    isOrdered = false;
                }
            }
            else
            {
                isOrdered = false;
            }

            return isOrdered;
        }

        internal void Add(KeyValuePair<string, double> pair)
        {
            var position = list.Count;

            while (position != 0 && IsOrdered(pair, list[position - 1]))
            {
                position--;
            }

            if (position < count)
            {
                if (list.Count == count)
                {
                    list.RemoveAt(count - 1);
                }

                list.Insert(position, pair);
            }
        }

        internal IEnumerable<string> GetTopScores()
        {
            return list.Select(r => r.Key);
        }
    }
}
