namespace TokenTreeTest
{
    public class TokenTreeNode : TokenTreeBase
    {
        public TokenTreeNode(string text, params TokenTreeNode[] children)
            : base(children)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
