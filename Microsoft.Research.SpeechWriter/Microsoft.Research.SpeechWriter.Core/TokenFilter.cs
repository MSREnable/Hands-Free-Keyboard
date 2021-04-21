namespace Microsoft.Research.SpeechWriter.Core
{
    internal class TokenFilter
    {
        internal TokenFilter()
        {
        }

        internal virtual bool IsVisible(int token)
        {
            return true;
        }
    }
}