using NUnit.Framework;

namespace TokenTreeTest
{
    public class Tests
    {
        private static void Check(TokenTreeRoot root, params string[] expecteds)
        {
            var actualsEnumerable = TokenTreeFormatter.Expand(root);

            var actuals = actualsEnumerable.ToArray();

            Assert.That(actuals.Length, Is.EqualTo(expecteds.Length));
            for (var i = 0; i < Math.Min(expecteds.Length, actuals.Length); i++)
            {
                var expected = expecteds[i];
                var actual = actuals[i];
                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [Test]
        public void CountingTest()
        {
            var root = new TokenTreeRoot(
                new TokenTreeNode("One", new TokenTreeNode("Two")),
                new TokenTreeNode("Three", new TokenTreeNode("Four")));
            Check(root, "[One] [Two]", "[Three] [Four]");
        }

        [Test]
        public void SimpleWordListTest()
        {
            var root = new TokenTreeRoot(
                new TokenTreeNode("Alpha"),
                new TokenTreeNode("Beta"),
                new TokenTreeNode("Gamma"));
            Check(root, "[Alpha]", "[Beta]", "[Gamma]");
        }

        [Test]
        public void TwoWordTest()
        {
            var root = new TokenTreeRoot(
                new TokenTreeNode("Good",
                    new TokenTreeNode("Afternoon"),
                    new TokenTreeNode("Evening"),
                    new TokenTreeNode("Morning")));
            Check(root, "[Good] [Afternoon]", "[Good Evening]", "[Good Morning]");
        }

        [Test]
        public void PrefixWordTest()
        {
            var root = new TokenTreeRoot(
                new TokenTreeNode("Alpha"),
                new TokenTreeNode("Go"),
                new TokenTreeNode("Goo"),
                new TokenTreeNode("Good", new TokenTreeNode("Heavens")),
                new TokenTreeNode("Zebra"));
            Check(root, "[Alpha]", "[Go] ]o] ]d] [Heavens]", "[Zebra]");
        }
    }
}