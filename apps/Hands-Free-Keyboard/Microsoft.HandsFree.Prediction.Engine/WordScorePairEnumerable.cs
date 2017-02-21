using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Engine.Properties;
using Microsoft.HandsFree.Prediction.Lucene.Internals;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.HandsFree.Prediction.Engine
{
    class WordScorePairEnumerable : Enumerable<WordScorePair>
    {
        internal static readonly WordScorePairEnumerable Instance = new WordScorePairEnumerable();

        WordScorePairEnumerable()
        {
        }

        string WordCountList { get { return Resources.WordCountList; } }

        public override IEnumerator<WordScorePair> GetEnumerator()
        {
            using (var reader = new StringReader(WordCountList))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var parts = line.Split('\t');
                    Debug.Assert(parts.Length == 2);

                    var word = parts[0];
                    var score = double.Parse(parts[1]);

                    var pair = new WordScorePair(word, score);

                    yield return pair;
                }
            }
        }

        public override int GetHashCode()
        {
            return WordCountList.GetHashCode();
        }
    }
}
