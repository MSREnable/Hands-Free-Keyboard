namespace TokenTreeTest
{
    public abstract class TokenTreeBase : ITokenTreeParent<string>
    {
        protected TokenTreeBase(params TokenTreeNode[] children)
        {
            Children = children;
        }

        public ITokenTreeNode<string>[] Children { get; }
    }
}
