namespace TokenTreeTest
{
    public interface ITokenTreeParent<TPayload>
    {
        ITokenTreeNode<TPayload>[] Children { get; }
    }
}