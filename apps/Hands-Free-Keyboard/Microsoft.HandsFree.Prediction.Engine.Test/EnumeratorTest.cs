namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    using Microsoft.HandsFree.Prediction.Api;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections;
    using System.Collections.Generic;

    [TestClass]
    public class EnumeratorTest
    {
        class OneTwoThreeEnumerable : Enumerable<int>
        {
            public override IEnumerator<int> GetEnumerator()
            {
                yield return 1;
                yield return 2;
                yield return 3;
            }
        }

        [TestMethod]
        public void GetEnumeratorTyped()
        {
            var enumerable = new OneTwoThreeEnumerable();

            // Get typed enumerator from IEnumerable<T> inerface.
            using (var enumerator = enumerable.GetEnumerator())
            {
                for (var i = 1; i <= 3; i++)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    Assert.AreEqual(i, enumerator.Current);
                }
                Assert.IsFalse(enumerator.MoveNext());
                Assert.IsFalse(enumerator.MoveNext());
            }
        }

        [TestMethod]
        public void GetEnumeratorUntyped()
        {
            var enumerable = new OneTwoThreeEnumerable();

            // Get untyped enumerator from IEnumerable interface.
            var enumerator = ((IEnumerable)enumerable).GetEnumerator();

            for (var i = 1; i <= 3; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(i, enumerator.Current);
            }
            Assert.IsFalse(enumerator.MoveNext());
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}
