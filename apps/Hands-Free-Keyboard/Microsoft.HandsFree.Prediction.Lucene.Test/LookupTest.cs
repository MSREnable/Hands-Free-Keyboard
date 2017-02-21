//using Lucene.Net.Analysis.Tokenattributes;
using Microsoft.HandsFree.Prediction.Engine;
using Microsoft.HandsFree.Prediction.Lucene.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.HandsFree.Prediction.Lucene.Test
{
    [TestClass]
    public class LookupTest
    {
        [TestMethod]
        public void CheckLookup()
        {
            var environment = new TestEnvironment();
            var index = WordIndexFactory.CreateFromWordCountList(environment, WordScorePairEnumerable.Instance);
            var results = index.Query("cloc");
            Assert.IsTrue(results.Count() != 0);
        }

        [TestMethod]
        public void CheckLookupWithBrokenCache()
        {
            var environment = new TestEnvironment();
            using (var stream = environment.CreateStaticDictionaryCache())
            {
                // Empty, invalid cache!
            }
            var index = WordIndexFactory.CreateFromWordCountList(environment, WordScorePairEnumerable.Instance);
            var results = index.Query("cloc");
            Assert.IsTrue(results.Count() != 0);
        }

        [TestMethod]
        public void FindTheClock()
        {
            var environment = new TestEnvironment();

            var wordScorePairs = new[]
            {
                new WordScorePair("clock", 1),
                new WordScorePair("onea", 0.2),
                new WordScorePair("oneb", 0.3),
                new WordScorePair("onec", 0.1),
                new WordScorePair("zzzz", 1),
                new WordScorePair("!!!", 1)
            };
            var index = WordIndexFactory.CreateFromWordCountList(environment, wordScorePairs);

            var results = index.Query("clo");
            Assert.IsTrue(results.Count() == 1);

            var resultsCloc = index.Query("cloc");
            Assert.IsTrue(resultsCloc.Count() == 1);
        }

        static void NGramCheck(Func<IEnumerable<WordScorePair>, IWordIndex> factory)
        {
            var wordScorePairs = new[]
            {
                new WordScorePair("onea", 0.2),
                new WordScorePair("oneb", 0.3),
                new WordScorePair("onec", 0.1),
            };

            var index = factory(wordScorePairs);

            var triple = index.Query("one");
            Assert.AreEqual(3, triple.Count(), "Should see three words with one in them");

            var single = index.Query("onea");
            Assert.IsTrue(1 <= single.Count(), "Should see only one word matching oneb");

            var bones = index.Query("bones");
            Assert.IsTrue(1 <= bones.Count(), "bones contains 'one'");

            var fullIndex = factory(WordScorePairEnumerable.Instance);
            var ones = index.Query("ones");
            Assert.IsTrue(1 <= ones.Count(), "Should be several words with 'ones' embedded in them");
        }

        [TestMethod]
        public void NGramJavaTest()
        {
            var environment = new TestEnvironment();
            NGramCheck((w) => WordIndexFactory.CreateFromWordCountListJava(environment, w));
        }

        [TestMethod]
        public void NGramCSharpTest()
        {
            var environment = new TestEnvironment();
            NGramCheck((w) => WordIndexFactory.CreateFromWordCountList(environment, w));
        }

        //[TestMethod]
        //public void TokenisationCheckCS()
        //{
        //    var analyser = new Microsoft.HandsFree.Prediction.LuceneDotNet.NGramAnalyzer();
        //    var reader = new StringReader("abcdef");

        //    var stream = analyser.TokenStream("wibble", reader);
        //    var term = stream.GetAttribute<ITermAttribute>();

        //    foreach (var expected in new[] { "abc", "bcd", "cde", "def", "abcd", "bcde", "cdef" })
        //    {
        //        Assert.IsTrue(stream.IncrementToken());
        //        Assert.AreEqual(expected, term.Term);
        //    }
        //    Assert.IsFalse(stream.IncrementToken());
        //}

        //[TestMethod]
        //public void TokenisationCheckJava()
        //{
        //    var analyser = new Microsoft.HandsFree.Prediction.Lucene.NGramAnalyzer();
        //    var reader = new java.io.StringReader("abcdef");

        //    var stream = analyser.tokenStream("wibble", reader);
        //    var offset = (org.apache.lucene.analysis.tokenattributes.OffsetAttribute)stream.getAttribute(typeof(org.apache.lucene.analysis.tokenattributes.OffsetAttribute));
        //    var term = (org.apache.lucene.analysis.tokenattributes.CharTermAttribute)stream.getAttribute(typeof(org.apache.lucene.analysis.tokenattributes.CharTermAttribute));

        //    stream.reset();
        //    foreach (var expected in new[] { "abc", "abcd", "bcd", "bcde", "cde", "cdef", "def" })
        //    {
        //        Assert.IsTrue(stream.incrementToken());

        //        var startOffset = offset.startOffset();
        //        var endOffset = offset.endOffset();
        //        var actual = "abcdef".Substring(startOffset, endOffset - startOffset);
        //        Assert.AreEqual(expected, actual);
        //    }
        //    Assert.IsFalse(stream.incrementToken());
        //}
    }
}
