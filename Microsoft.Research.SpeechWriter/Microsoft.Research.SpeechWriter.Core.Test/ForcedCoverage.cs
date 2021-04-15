using Microsoft.Research.SpeechWriter.Core.Items;
using NUnit.Framework;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    class ForcedCoverage
    {
        [Test]
        public void InterstitialNonItemCoverage()
        {
            var nonItem = (ITile)new InterstitialNonItem();
            Assert.AreEqual("~", nonItem.Content);
            Assert.IsFalse(nonItem.CanExecute(null));
            nonItem.Execute(null);
        }

        [Test]
        public void InterstitialSpellingItemCoverage()
        {
            var spellingItem = new InterstitialSpellingItem(null, null, 0);
            Assert.AreEqual("*", spellingItem.Content);
        }

        [Test]
        public void InterstitialGapItemCoverage()
        {
            var gapItem = new InterstitialGapItem(null, null, null, 0, 1);
            Assert.AreEqual(":", gapItem.Content);
        }
    }
}
