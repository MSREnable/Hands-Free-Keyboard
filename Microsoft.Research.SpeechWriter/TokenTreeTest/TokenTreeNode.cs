namespace TokenTreeTest
{
    public class TokenTreeNode<TPayload> : TokenTreeBase<TPayload>, ITokenTreeNode<TPayload>
        where TPayload : ITreeToken<TPayload>
    {
        public TokenTreeNode(TPayload payload, params ITokenTreeNode<TPayload>[] children)
            : base(children)
        {
            Payload = payload;
        }

        public TPayload Payload { get; }
    }
}
