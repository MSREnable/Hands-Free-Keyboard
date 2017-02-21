using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.HandsFree.Prediction.Historic
{
    class KeyScoreOrderedList
    {
        class ScoreKeyComparer : IComparer<KeyScorePair>
        {
            internal static ScoreKeyComparer instance = new ScoreKeyComparer();

            public int Compare(KeyScorePair x, KeyScorePair y)
            {
                var comparison = y.score.CompareTo(x.score);
                if (comparison == 0)
                {
                    comparison = x.key.CompareTo(y.key);
                }
                return comparison;
            }
        }

        /// <summary>
        /// Score ordered keys.
        /// </summary>
        readonly SortedSet<KeyScorePair> scoredKeys = new SortedSet<KeyScorePair>(ScoreKeyComparer.instance);

        /// <summary>
        /// Dictionary of known scores.
        /// </summary>
        readonly IDictionary<int, KeyScorePair> dictionary = new Dictionary<int, KeyScorePair>();

        /// <summary>
        /// Incorporate new key-score pair into list.
        /// </summary>
        /// <param name="key">The word to be added.</param>
        /// <param name="score">The score to be added to the key.</param>
        public void Include(int key, double score)
        {
            KeyScorePair pair;
            if (!dictionary.TryGetValue(key, out pair))
            {
                pair = new KeyScorePair
                {
                    key = key,
                    score = score
                };
                dictionary.Add(key, pair);
            }
            else
            {
                scoredKeys.Remove(pair);
                pair.score += score;
            }
            scoredKeys.Add(pair);
        }

        /// <summary>
        /// Combine another order list into this.
        /// </summary>
        /// <param name="list">The list to incoroporate.</param>
        public void Include(KeyScoreOrderedList list)
        {
            foreach (var pair in list.scoredKeys)
            {
                Include(pair.key, pair.score);
            }
        }

        /// <summary>
        /// Enumerator for contents of list.
        /// </summary>
        public IEnumerable<KeyScorePair> KeyScorePairs
        {
            get
            {
                foreach (var pair in scoredKeys)
                {
                    yield return pair;
                }
            }
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write((Int32)scoredKeys.Count);

            foreach (var pair in scoredKeys)
            {
                writer.Write(pair.key);
                writer.Write((double)pair.score);
            }
        }

        internal static KeyScoreOrderedList Read(BinaryReader reader)
        {
            var list = new KeyScoreOrderedList();

            var scoredKeysCount = reader.ReadInt32();

            for (var index = 0; index < scoredKeysCount; index++)
            {
                var key = reader.ReadInt32();
                var score = reader.ReadDouble();

                list.Include(key, score);
            }

            return list;
        }
    }
}
