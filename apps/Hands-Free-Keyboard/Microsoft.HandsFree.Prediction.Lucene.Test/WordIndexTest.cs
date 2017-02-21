using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Lucene.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Prediction.Lucene.Test
{
    [TestClass]
    public class WordIndexTest
    {
        [TestMethod]
        public void CreateIndex()
        {
            var environment = new TestEnvironment();

            var wordScorePairs = new[]
            {
                new WordScorePair("onea", 0.2),
                new WordScorePair("oneb", 0.3),
                new WordScorePair("onec", 0.1),
                new WordScorePair("zzzz", 1),
                new WordScorePair("!!!", 1)
            };
            var index = WordIndexFactory.CreateFromWordCountListJava(environment, wordScorePairs);

            var results = index.Query("one");

            using (var enumerator = results.GetEnumerator())
            {
                Assert.IsTrue(enumerator.MoveNext(), "First item");
                Assert.AreEqual("oneb", enumerator.Current, "0.3 weighted");

                Assert.IsTrue(enumerator.MoveNext(), "Second item");
                Assert.AreEqual("onea", enumerator.Current, "0.2 weighted");

                Assert.IsTrue(enumerator.MoveNext(), "Third item");
                Assert.AreEqual("onec", enumerator.Current, "0.1 weighted");

                Assert.IsFalse(enumerator.MoveNext(), "That's all!");
            }
        }

        //static void CheckCharacterCase(IWordIndex index, string queryString, params string[] expecteds)
        //{
        //    var actuals = index.Query(queryString);

        //    using (var enumerator = actuals.GetEnumerator())
        //    {
        //        foreach (var expected in expecteds)
        //        {
        //            Assert.IsTrue(enumerator.MoveNext(), "Value present");

        //            var actual = enumerator.Current;
        //            Assert.AreEqual(expected, actual, "Right value");
        //        }

        //        Assert.IsFalse(enumerator.MoveNext(), "That's all");
        //    }
        //}

        //[TestMethod]
        //public void CheckCharacterCase()
        //{
        //    var environment = new TestEnvironment();
        //    var index = WordIndexFactory.CreateFromWordCountListJava(environment, new WordScorePair[0]);

        //    CheckCharacterCase(index, "one", "oneb", "onea", "onec");
        //}

        [TestMethod]
        public void CheckPunctuation()
        {
            Assert.IsTrue(WordAndPunctuationHelper.IsSentenceEnding(null));
            Assert.IsTrue(string.Empty.IsSentenceEnding());
            Assert.IsTrue(".".IsSentenceEnding());
            Assert.IsFalse(" ".IsSentenceEnding());
        }
    }
}
