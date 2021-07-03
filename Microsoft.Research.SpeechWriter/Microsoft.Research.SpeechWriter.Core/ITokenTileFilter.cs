namespace Microsoft.Research.SpeechWriter.Core
{
    internal interface ITokenTileFilter : ITileFilter
    {
        bool IsTokenVisible(int token);
    }
}