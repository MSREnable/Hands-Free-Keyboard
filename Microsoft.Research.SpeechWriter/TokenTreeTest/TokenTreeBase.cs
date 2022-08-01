namespace TokenTreeTest
{
    public abstract class TokenTreeBase
    {
        protected TokenTreeBase(params TokenTreeNode[] children)
        {
            Children = children;
        }

        public TokenTreeNode[] Children { get; }
    }
}
