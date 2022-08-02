namespace TokenTreeTest
{
    public interface ITokenTreeParent<TPayload>
        where TPayload : ITreeToken<TPayload>
    {
        ITokenTreeNode<TPayload>[] Children { get; }
    }
}