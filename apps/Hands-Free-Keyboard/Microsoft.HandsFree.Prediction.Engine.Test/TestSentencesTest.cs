namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    using Microsoft.HandsFree.Prediction.Api;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections;

    [TestClass]
    public class TestSentencesTest
    {
        [TestMethod]
        public void Enumerableness()
        {
            var enumerable = (IEnumerable)TestSentences.Instance;

            var position = 0;
            foreach (string s in enumerable)
            {
                Assert.AreEqual(TestSentences.Instance[position], s);
                position++;
            }

            Assert.AreEqual(TestSentences.Instance.Count, position);
        }
    }
}
