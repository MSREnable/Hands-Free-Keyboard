namespace Microsoft.Research.SpeechWriter.Core
{
    internal class RepeatTokenFilter
    {
        public RepeatTokenFilter()
        {
        }

        internal virtual bool Accept(int token)
        {
            return true;
        }
    }
}