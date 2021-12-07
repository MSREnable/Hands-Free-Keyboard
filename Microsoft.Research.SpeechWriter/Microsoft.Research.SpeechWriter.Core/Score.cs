namespace Microsoft.Research.SpeechWriter.Core
{
    internal struct Score
    {
        private readonly int[] _values;

        internal Score(params int[] values)
        {
            _values = values;
        }

        internal int[] Values => _values;
    }
}
