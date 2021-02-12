using Microsoft.Research.RankWriter.Library.Items;
using NUnit.Framework;
using System.Windows.Input;

namespace Microsoft.Research.RankWriter.Library.Test
{
    class ForcedCoverage
    {
        [Test]
        public void InterstitialNonItemCoverage()
        {
            var nonItem = (ICommand)new InterstitialNonItem();
            Assert.AreEqual("~", nonItem.ToString());
            Assert.IsFalse(nonItem.CanExecute(null));
            nonItem.Execute(null);
        }

        [Test]
        public void InterstitialSpellingItemCoverage()
        {
            var spellingItem = new InterstitialSpellingItem(null, 0);
            Assert.AreEqual("*", spellingItem.ToString());
        }

        [Test]
        public void InterstitialGapItemCoverage()
        {
            var gapItem = new InterstitialGapItem(null, null, 0, 1);
            Assert.AreEqual(":", gapItem.ToString());
        }
    }
}
