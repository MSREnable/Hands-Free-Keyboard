namespace TokenTreeTest
{
    public interface ITokenTreeNode<TPayload> : ITokenTreeParent<TPayload>
    {
        public TPayload Payload { get; }
    }
}