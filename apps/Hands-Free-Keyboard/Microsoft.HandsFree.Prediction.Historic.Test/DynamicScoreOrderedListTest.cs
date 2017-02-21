namespace Microsoft.HandsFree.Prediction.Historic.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DynamicScoreOrderedListTest
    {
        static void Check(KeyScoreOrderedList list, params string[] expectations)
        {
            using (var enumerator = list.KeyScorePairs.GetEnumerator())
            {
                foreach (var expected in expectations)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    Assert.AreEqual(expected, enumerator.Current.key);
                }
                Assert.IsFalse(enumerator.MoveNext());
            }
        }

        [TestMethod]
        public void PopulateScoreOrderedList()
        {
            var list = new KeyScoreOrderedList();

            Check(list);

            //list.Include("B", 1.01);
            //Check(list, "B");

            //list.Include("A", 1.02);
            //Check(list, "A", "B");

            //list.Include("C", 1.04);
            //Check(list, "C", "A", "B");

            //list.Include("A", 1.08);
            //Check(list, "A", "C", "B");

            //list.Include("B", 1.16);
            //Check(list, "B", "A", "C");

            //list.Include("A", 1.32);
            //Check(list, "A", "B", "C");
        }
    }
}
