namespace TokenTreeTest
{
    public interface ITreeToken<TPayload>
        where TPayload : ITreeToken<TPayload>
    {
        TPayload? Join(TPayload token);

        TPayload? Suffix(TPayload token);
    }
}
