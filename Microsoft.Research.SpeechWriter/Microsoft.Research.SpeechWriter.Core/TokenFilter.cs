namespace Microsoft.Research.SpeechWriter.Core
{
    internal class TokenFilter
    {
        public TokenFilter()
        {
        }

        internal virtual bool Accept(int token)
        {
            return true;
        }
    }
}