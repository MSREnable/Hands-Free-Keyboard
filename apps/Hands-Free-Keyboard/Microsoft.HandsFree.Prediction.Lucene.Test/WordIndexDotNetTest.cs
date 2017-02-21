using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Engine;
using Microsoft.HandsFree.Prediction.Lucene.Internals;
using Microsoft.HandsFree.Prediction.Lucene.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Microsoft.HandsFree.Prediction.LuceneDotNet.Test
{
    [TestClass]
    public class WordIndexDotNetTest
    {
        //[TestMethod]
        //public void CreateIndex()
        //{
        //    var wordScorePairs = new[]
        //    {
        //        new WordScorePair("onea", 0.2),
        //        new WordScorePair("oneb", 0.3),
        //        new WordScorePair("onec", 0.1),
        //        new WordScorePair("zzzz", 1),
        //        new WordScorePair("!!!", 1)
        //    };
        //    var index = WordIndexFactory.CreateFromWordCountList(TestEnvironment.Instance, wordScorePairs, string.Empty);

        //    var results = index.Query("one");

        //    using (var enumerator = results.GetEnumerator())
        //    {
        //        Assert.IsTrue(enumerator.MoveNext(), "First item");
        //        Assert.AreEqual("oneb", enumerator.Current, "0.3 weighted");

        //        Assert.IsTrue(enumerator.MoveNext(), "Second item");
        //        Assert.AreEqual("onea", enumerator.Current, "0.2 weighted");

        //        Assert.IsTrue(enumerator.MoveNext(), "Third item");
        //        Assert.AreEqual("onec", enumerator.Current, "0.1 weighted");

        //        Assert.IsFalse(enumerator.MoveNext(), "That's all!");
        //    }
        //}

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
        //    var index = WordIndexFactory.CreateFromWordCountList(TestEnvironment.Instance, new WordScorePair[0], "onec onea onea oneb oneb oneb");

        //    CheckCharacterCase(index, "one", "oneb", "onea", "onec");
        //}

        //[TestMethod]
        //public void CheckPunctuation()
        //{
        //    Assert.IsTrue(WordAndPunctuationHelper.IsSentenceEnding(null));
        //    Assert.IsTrue(string.Empty.IsSentenceEnding());
        //    Assert.IsTrue(".".IsSentenceEnding());
        //    Assert.IsFalse(" ".IsSentenceEnding());
        //}

        //[TestMethod]
        //public void Caching()
        //{
        //    var environment = new TestEnvironment();

        //    Assert.IsNull(environment.OpenDictionaryCache(), "No initial cache");

        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    var indexFromScratch = WordIndexFactory.CreateFromWordCountList(environment, WordScorePairEnumerable.Instance, string.Empty);

        //    var buildFromScratchTime = stopwatch.Elapsed;

        //    stopwatch.Restart();

        //    var indexFromCache = WordIndexFactory.CreateFromWordCountList(environment, WordScorePairEnumerable.Instance, string.Empty);

        //    var buildFromCacheTime = stopwatch.Elapsed;

        //    Assert.IsTrue(buildFromCacheTime < buildFromScratchTime, "Should be quicker to load from cache than to build from scratch");
        //}

        //[TestMethod]
        //public void BrokenCache()
        //{
        //    var environment = new TestEnvironment();

        //    Assert.IsNull(environment.OpenDictionaryCache(), "No initial cache");
        //    using (var stream = environment.CreateDictionaryCache())
        //    {
        //        // An empty cache is a broken cache!
        //    }

        //    var index = WordIndexFactory.CreateFromWordCountList(environment, WordScorePairEnumerable.Instance, string.Empty);

        //    // That should not throw!
        //}
    }
}
