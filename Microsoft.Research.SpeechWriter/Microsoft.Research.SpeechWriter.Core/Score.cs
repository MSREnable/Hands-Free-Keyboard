namespace Microsoft.Research.SpeechWriter.Core
{
    internal struct Score
    {
        private readonly int[] _values;

        internal Score(params int[] values)
        {
            _values = values;
        }

        internal int Token => _values[0];

        internal int Length => _values.Length;

        internal int this[int index] => _values[index];
    }
}
