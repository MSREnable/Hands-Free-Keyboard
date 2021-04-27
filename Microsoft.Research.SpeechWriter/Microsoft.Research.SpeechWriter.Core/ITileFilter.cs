namespace Microsoft.Research.SpeechWriter.Core
{
    internal interface ITileFilter
    {
        bool IsIndexVisible(int index);
    }
}