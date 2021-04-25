namespace Microsoft.Research.SpeechWriter.Core
{
    internal class TileFilter
    {
        internal TileFilter()
        {
        }

        internal virtual bool IsTokenVisible(int token)
        {
            return true;
        }
    }
}