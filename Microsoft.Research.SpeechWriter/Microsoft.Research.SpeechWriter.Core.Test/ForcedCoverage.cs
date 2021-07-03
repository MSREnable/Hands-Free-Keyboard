using Microsoft.Research.SpeechWriter.Core.Items;
using NUnit.Framework;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    internal class ForcedCoverage
    {
        [Test]
        public void InterstitialNonItemCoverage()
        {
            var model = new ApplicationModel();
            var nonItem = (ITile)new InterstitialNonItem(model);
            Assert.AreEqual("~", nonItem.Content);
            Assert.IsFalse(nonItem.CanExecute(null));
            nonItem.Execute(null);
        }

        //[Test]
        //public void InterstitialSpellingItemCoverage()
        //{
        //    var spellingItem = new InterstitialSpellingItem(null, null, 0);
        //    Assert.AreEqual("*", spellingItem.Content);
        //}

        [Test]
        public void InterstitialGapItemCoverage()
        {
            var model = new ApplicationModel();
            var gapItem = new InterstitialGapItem(null, model, model.Source, 0, 1);
            Assert.AreEqual(":", gapItem.Content);
        }
    }
}
