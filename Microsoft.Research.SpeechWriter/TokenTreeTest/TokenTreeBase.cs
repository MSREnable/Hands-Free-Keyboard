namespace TokenTreeTest
{
    public abstract class TokenTreeBase<TPayload> : ITokenTreeParent<TPayload>
        where TPayload : ITreeToken<TPayload>
    {
        protected TokenTreeBase(params ITokenTreeNode<TPayload>[] children)
        {
            Children = children;
        }

        public ITokenTreeNode<TPayload>[] Children { get; }
    }
}
