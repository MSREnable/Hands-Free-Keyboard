using Microsoft.HandsFree.Prediction.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    [TestClass]
    public class EmptyEnumeratorTest
    {
        [TestMethod]
        public void TypedEmptyEnumeratorTest()
        {
            for (var i = 0; i < 3; i++)
            {
                using (var enumerator = EmptySuggestions.Instance.GetEnumerator())
                {
                    Assert.IsFalse(enumerator.MoveNext());
                    Assert.IsFalse(enumerator.MoveNext());
                }
            }

            Assert.AreEqual(0, EmptySuggestions.Instance.Context.Length, "No context");
        }

        [TestMethod]
        public void UntypedEmptyEnumeratorTest()
        {
            for (var i = 0; i < 3; i++)
            {
                var enumerator = ((IEnumerable)EmptySuggestions.Instance).GetEnumerator();

                Assert.IsFalse(enumerator.MoveNext());
                Assert.IsFalse(enumerator.MoveNext());
            }
        }
    }
}
