namespace Microsoft.HandsFree.Prediction.Historic.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringInternerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var s1 = 42.ToString();
            var s2 = 42.ToString();

            Assert.AreNotSame(s1, s2, "Should have two objects");

            var t1 = string.Intern(s1);
            var t2 = string.Intern(s2);

            Assert.AreEqual(s1, t1, "Same value");
            Assert.AreEqual(s2, t2, "Same value");
            Assert.AreSame(t1, t2, "Should have one object");
        }
    }
}
