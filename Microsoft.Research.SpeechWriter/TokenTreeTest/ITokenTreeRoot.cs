namespace TokenTreeTest
{
    internal interface ITokenTreeRoot<TPayload> : ITokenTreeParent<TPayload>
        where TPayload : ITreeToken<TPayload>
    {
    }
}