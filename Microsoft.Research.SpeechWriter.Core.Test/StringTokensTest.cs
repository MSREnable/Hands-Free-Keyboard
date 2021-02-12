using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Research.RankWriter.Library.Test
{
    public class StringTokensTest
    {
        [Test]
        public void ConstructionTest()
        {
            var words = DefaultWriterEnvironment.Instance.GetOrderedSeedWords();
            var tokens = StringTokens.Create(words);
            Assert.IsNotNull(tokens);

            for (var index = tokens.TokenLimit - 1; tokens.TokenStart <= index; index--)
            {
                var str = tokens[index];

                var token = tokens[str];
                Assert.AreEqual(index, token);

                var roundTrip = tokens[token];
                Assert.AreEqual(str, roundTrip);
            }

            var oldTokenLimit = tokens.TokenLimit;
            var newToken = tokens.GetToken("new lowercase string");
            Assert.AreEqual(oldTokenLimit, newToken);
            Assert.AreEqual(oldTokenLimit + 1, tokens.TokenLimit);
        }

        private static void CheckSentences(StringTokens tokens, string caption)
        {
            Debug.WriteLine(caption);
            Debug.Indent();
            foreach (var sentence in DefaultWriterEnvironment.Instance.GetSeedSentences())
            {
                Assert.IsNotNull(sentence);
                Assert.IsTrue(1 < sentence.Count());

                var sequence = new List<int>(new[] { 0 });
                foreach(var word in sentence)
                {
                    var token = tokens.GetToken(word);
                    sequence.Add(token);
                }
                Assert.AreEqual(0, sequence[0]);

                for (var i = 1; i < sequence.Count; i++)
                {
                    var token = sequence[i];
                    var word = tokens[token];
                    Debug.Write($"{word}({token}) ");
                }
                Debug.WriteLine(".");
            }
            Debug.Unindent();
        }

        [Test]
        public void EmptySentencesTest()
        {
            var tokens = new StringTokens();
            CheckSentences(tokens, "Empty tokens");
        }

        [Test]
        public void InitializedSentencesTest()
        {
            var words = DefaultWriterEnvironment.Instance.GetOrderedSeedWords();
            var tokens = StringTokens.Create(words);
            CheckSentences(tokens, "Initialized tokens");
        }
    }
}
