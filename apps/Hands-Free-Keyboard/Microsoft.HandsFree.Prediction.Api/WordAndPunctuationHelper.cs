using System;

namespace Microsoft.HandsFree.Prediction.Api
{
    /// <summary>
    /// Helper class for splitting strings into word parts and punctuation parts.
    /// </summary>
    public static class WordAndPunctuationHelper
    {
        /// <summary>
        /// Characters in punctuation that end a sentence.
        /// </summary>
        static readonly char[] sentenceEndingPunctuation = new[] { '.', '\r', '\n', ':', '?', '!' };

        /// <summary>
        /// Test whether a character is a letter or not.
        /// </summary>
        /// <param name="s">String containing character.</param>
        /// <param name="index">Position in string of character.</param>
        /// <returns>True iff character is a letter.</returns>
        static bool IsLetter(string s, int index)
        {
            return char.IsLetter(s, index);
        }

        static bool IsSurrogatePair(string s, int index)
        {
            if (s.Length > 1)
            {
                // Edge Case: Index is at the beginning of string (index must be first surrogate of the surrogate pair)
                if (index == 0)
                {
                    return char.IsSurrogatePair(s, index);
                }
                // Edge Case: Index is at the end of the string (index must be second surrogate of the surrogate pair)
                else if (index == s.Length - 1)
                {
                    return char.IsSurrogatePair(s, index - 1);
                }
                // General Case: Index is at least 1 from the start or end of the string (index may be first or second surrogate of the surrogate pair)
                else
                {
                    return char.IsSurrogatePair(s, index) || char.IsSurrogatePair(s, index - 1);
                }
            }

            return false;
        }

        /// <summary>
        /// Test whether a character, if it occurs as the sole character in a string 
        /// between two words, joins those words together.
        /// </summary>
        /// <param name="s">String containing character.</param>
        /// <param name="startIndex">Position in string of character.</param>
        /// <returns>True iff character can be used to join two words together.</returns>
        static bool IsWordJoin(string s, int startIndex)
        {
            bool isWordJoin;

            switch (s[startIndex])
            {
                case '\'':
                case '’':
                case '-':
                    isWordJoin = true;
                    break;

                default:
                    isWordJoin = false;
                    break;
            }

            return isWordJoin;
        }

        /// <summary>
        /// Gets the length of a piece of punctuation in a string.
        /// </summary>
        /// <param name="s">The string containing the punctuation.</param>
        /// <param name="startIndex">The position the punctuation starts.</param>
        /// <returns>The length of the punctuation.</returns>
        public static int PunctuationLength(this string s, int startIndex)
        {
            var endIndex = startIndex;
            while (endIndex < s.Length && !IsLetter(s, endIndex))
            {
                endIndex++;
            }

            var length = endIndex - startIndex;

            return length;
        }

        /// <summary>
        /// Gets the length of a piece of punctuation in a string, starting from its end.
        /// </summary>
        /// <param name="s">The string containing the punctuation.</param>
        /// <param name="startIndex">The position just beyond where the punctuation ends.</param>
        /// <returns>The length of the punctuation.</returns>
        public static int ReversePunctuationLength(this string s, int startIndex)
        {
            var endIndex = startIndex;
            while (endIndex != 0 && !IsLetter(s, endIndex - 1))
            {
                endIndex--;
            }

            var length = startIndex - endIndex;

            return length;
        }

        static int GetSimpleWordLength(string s, int startIndex)
        {
            var endIndex = startIndex;
            while (endIndex < s.Length && IsLetter(s, endIndex))
            {
                endIndex++;
            }

            var length = endIndex - startIndex;

            return length;
        }

        static int GetSimpleWordLengthReverse(string s, int startIndex)
        {
            var endIndex = startIndex;
            while (endIndex != 0 && IsLetter(s, endIndex - 1))
            {
                endIndex--;
            }

            var length = startIndex - endIndex;

            return length;
        }

        static int GetFollowOnLength(string s, int startIndex)
        {
            int length;

            if (startIndex < s.Length && IsWordJoin(s, startIndex))
            {
                var wordLength = GetSimpleWordLength(s, startIndex + 1);
                if (wordLength != 0)
                {
                    length = wordLength + 1;
                }
                else
                {
                    length = 0;
                }
            }
            else
            {
                length = 0;
            }

            return length;
        }

        static int GetFollowOnLengthReverse(string s, int startIndex)
        {
            int length;

            if (startIndex != 0 && IsWordJoin(s, startIndex - 1))
            {
                var wordLength = GetSimpleWordLengthReverse(s, startIndex - 1);
                if (wordLength != 0)
                {
                    length = wordLength + 1;
                }
                else
                {
                    length = 0;
                }
            }
            else
            {
                length = 0;
            }

            return length;
        }

        /// <summary>
        /// Gets the length of a word within a string.
        /// </summary>
        /// <param name="s">The string containing the word.</param>
        /// <param name="startIndex">The position within the string the word starts.</param>
        /// <returns>The length of the word.</returns>
        public static int WordLength(this string s, int startIndex)
        {
            var length = GetSimpleWordLength(s, startIndex);

            if (length != 0)
            {
                for (var followOnLength = GetFollowOnLength(s, startIndex + length);
                    followOnLength != 0;
                    followOnLength = GetFollowOnLength(s, startIndex + length))
                {
                    length += followOnLength;
                }
            }

            return length;
        }

        /// <summary>
        /// Gets the length of a word within a string starting from its end.
        /// </summary>
        /// <param name="s">The string containing the word.</param>
        /// <param name="startIndex">The position within the string the word ends.</param>
        /// <returns>The length of the word.</returns>
        public static int ReverseWordLength(this string s, int startIndex)
        {
            var length = GetSimpleWordLengthReverse(s, startIndex);

            if (length != 0)
            {
                for (var followOnLength = GetFollowOnLengthReverse(s, startIndex - length);
                    followOnLength != 0;
                    followOnLength = GetFollowOnLengthReverse(s, startIndex - length))
                {
                    length += followOnLength;
                }
            }

            return length;
        }

        public static bool IsSentenceEnding(this string s)
        {
            return string.IsNullOrEmpty(s) || s.IndexOfAny(sentenceEndingPunctuation) != -1;
        }

        public static bool IsSentenceEnding(this char ch)
        {
            return Array.IndexOf(sentenceEndingPunctuation, ch) != -1;
        }
    }
}
