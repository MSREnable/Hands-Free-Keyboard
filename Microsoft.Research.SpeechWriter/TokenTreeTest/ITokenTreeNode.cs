namespace TokenTreeTest
{
    public interface ITokenTreeNode<TPayload> : ITokenTreeParent<TPayload>
        where TPayload : ITreeToken<TPayload>
    {
        public TPayload Payload { get; }
    }
}