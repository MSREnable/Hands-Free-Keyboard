using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class WordCaseMap
    {
        private readonly int[] _positions;
        private readonly bool[] _uppers;
        private readonly int _lowersCount;

        private WordCaseMap(int[] positions, bool[] uppers, int lowersCount)
        {
            _positions = positions;
            _uppers = uppers;
            _lowersCount = lowersCount;
        }

        internal int LetterCount => _positions.Length;

        internal int LowerCount => _lowersCount;

        internal int UpperCount => _positions.Length - _lowersCount;

        internal int[] Positions => _positions;

        internal bool[] Uppers => _uppers;

        internal static WordCaseMap Create(string word)
        {
            var lowerCount = 0;
            var upperCount = 0;
            var positions = new List<int>();
            var uppers = new List<bool>();

            for (var position = 0; position < word.Length; position++)
            {
                var ch = word[position];

                if (char.IsLetter(ch))
                {
                    var lower = char.ToLower(ch);
                    var upper = char.ToUpper(ch);
                    if (lower != upper)
                    {
                        if (ch == lower)
                        {
                            if (ch != upper)
                            {
                                lowerCount++;
                                positions.Add(position);
                                uppers.Add(false);
                            }
                        }
                        else if (ch == upper)
                        {
                            if (ch != lower)
                            {
                                upperCount++;
                                positions.Add(position);
                                uppers.Add(true);
                            }
                        }
                    }
                }
            }

            Debug.Assert(lowerCount + upperCount == positions.Count);
            Debug.Assert(positions.Count == uppers.Count);

            var map = new WordCaseMap(positions.ToArray(), uppers.ToArray(), lowerCount);
            return map;
        }

        internal int GetDistanceTo(WordCaseMap other)
        {
            var distance = 0;

            Debug.Assert(Positions.Length == other.Positions.Length);

            for (var i = 0; i < Positions.Length; i++)
            {
                Debug.Assert(Positions[i] == other.Positions[i]);

                if (Uppers[i] != other.Uppers[i])
                {
                    distance++;
                }
            }

            return distance;
        }
    }
}
