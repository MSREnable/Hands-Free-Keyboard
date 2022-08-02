using NUnit.Framework;

namespace TokenTreeTest
{
    public class Tests
    {
        private static void Check(ITokenTreeRoot<StringTreeToken> root, params string[] expecteds)
        {
            var actualsEnumerable = TokenTreeFormatter.Expand(root);

            var actuals = actualsEnumerable.ToArray();

            Assert.That(actuals.Length, Is.EqualTo(expecteds.Length));
            for (var i = 0; i < Math.Min(expecteds.Length, actuals.Length); i++)
            {
                var expected = expecteds[i];
                var actual = actuals[i];

                var text = actual[0].ToString();
                for (var j = 1; j < actual.Count; j++)
                {
                    text += ' ' + actual[j].ToString();
                }
                Assert.That(text, Is.EqualTo(expected));
            }
        }

        private static ITokenTreeRoot<StringTreeToken> CreateRoot(params ITokenTreeNode<StringTreeToken>[] nodes)
        {
            var root = new TokenTreeRoot<StringTreeToken>(nodes);
            return root;
        }

        private static ITokenTreeNode<StringTreeToken> CreateNode(string text, params ITokenTreeNode<StringTreeToken>[] nodes)
        {
            var token = new StringTreeToken(text);
            var node = new TokenTreeNode<StringTreeToken>(token, nodes);
            return node;
        }

        [Test]
        public void CountingTest()
        {
            var root = CreateRoot(
                CreateNode("One", CreateNode("Two")),
                CreateNode("Three", CreateNode("Four")));
            Check(root, "[One] [Two]", "[Three] [Four]");
        }

        [Test]
        public void SimpleWordListTest()
        {
            var root = CreateRoot(
                CreateNode("Alpha"),
                CreateNode("Beta"),
                CreateNode("Gamma"));
            Check(root, "[Alpha]", "[Beta]", "[Gamma]");
        }

        [Test]
        public void TwoWordTest()
        {
            var root = CreateRoot(
                CreateNode("Good",
                    CreateNode("Afternoon"),
                    CreateNode("Evening"),
                    CreateNode("Morning")));
            Check(root, "[Good] [Afternoon]", "[Good Evening]", "[Good Morning]");
        }

        [Test]
        public void PrefixWordTest()
        {
            var root = CreateRoot(
                CreateNode("Alpha"),
                CreateNode("Go"),
                CreateNode("Goo"),
                CreateNode("Good", CreateNode("Heavens")),
                CreateNode("Zebra"));
            Check(root, "[Alpha]", "[Go] ]o] ]d] [Heavens]", "[Zebra]");
        }
    }
}