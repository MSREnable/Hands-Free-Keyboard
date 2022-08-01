namespace TokenTreeTest
{
    public class TokenTreeNode : TokenTreeBase, ITokenTreeNode<string>
    {
        public TokenTreeNode(string text, params TokenTreeNode[] children)
            : base(children)
        {
            Payload = text;
        }

        public string Payload { get; }
    }
}
