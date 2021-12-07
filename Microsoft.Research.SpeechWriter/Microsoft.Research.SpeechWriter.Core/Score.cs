using System;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal struct Score : IComparable<Score>
    {
        private readonly int[] _values;

        internal Score(params int[] values)
        {
            _values = values;
        }

        internal int Token => _values[0];

        internal int Length => _values.Length;

        internal int this[int index] => _values[index];

        public static bool operator ==(Score lhs, Score rhs) => lhs.CompareTo(rhs) == 0;
        public static bool operator !=(Score lhs, Score rhs) => lhs.CompareTo(rhs) != 0;
        public static bool operator <(Score lhs, Score rhs) => lhs.CompareTo(rhs) < 0;
        public static bool operator <=(Score lhs, Score rhs) => lhs.CompareTo(rhs) <= 0;
        public static bool operator >=(Score lhs, Score rhs) => lhs.CompareTo(rhs) >= 0;
        public static bool operator >(Score lhs, Score rhs) => lhs.CompareTo(rhs) > 0;

        public int CompareTo(Score other)
        {
            var length = _values.Length;
            var value = length.CompareTo(other._values.Length);

            if (value == 0)
            {
                var position = length - 1;
                while (0 < position && _values[position] == other._values[position])
                {
                    position--;
                }

                value = _values[position].CompareTo(other._values[position]);
            }

            return value;
        }

        public override bool Equals(object obj)
        {
            var value = obj is Score other && CompareTo(other) == 0;
            return value;
        }

        public override int GetHashCode() => Token;
    }
}
