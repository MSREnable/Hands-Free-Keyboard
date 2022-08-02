namespace TokenTreeTest
{
    internal class TokenTreeRoot<TPayload> : TokenTreeBase<TPayload>, ITokenTreeRoot<TPayload>
        where TPayload : ITreeToken<TPayload>
    {
        public TokenTreeRoot(params ITokenTreeNode<TPayload>[] children)
            : base(children)
        {

        }
    }
}
