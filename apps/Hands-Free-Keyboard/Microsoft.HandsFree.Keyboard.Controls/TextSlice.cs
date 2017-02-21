using System;
using System.Diagnostics;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Selection within a text string.
    /// </summary>
    public class TextSlice
    {
        /// <summary>
        /// An empty text slice.
        /// </summary>
        public static readonly TextSlice Empty = new TextSlice(string.Empty, 0, 0, true);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">The text being sliced.</param>
        /// <param name="start">Start index of slice.</param>
        /// <param name="length">Length of slice.</param>
        /// <param name="isCaretAtEnd">Is caret at the end of the selection.</param>
        public TextSlice(string text, int start, int length, bool isCaretAtEnd)
        {
            Debug.Assert(text != null);
            Debug.Assert(0 <= start);
            Debug.Assert(0 <= length);
            Debug.Assert(start + length <= text.Length);

            Text = text;
            Start = start;
            Length = length;
            IsCaretAtEnd = isCaretAtEnd;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">The text being sliced.</param>
        /// <param name="start">Start index of slice.</param>
        /// <param name="length">Length of slice.</param>
        [Obsolete]
        public TextSlice(string text, int start, int length)
            : this(text, start, length, true)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">The text being sliced.</param>
        /// <param name="start">Start index of slice.</param>
        public TextSlice(string text, int start)
            : this(text, start, 0, true)
        {
        }

        /// <summary>
        /// The sliced text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Start of slice.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Length of slice.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Is the caret at the end rather than the start of the selection?
        /// </summary>
        public bool IsCaretAtEnd { get; }

        /// <summary>
        /// Part before selection.
        /// </summary>
        public string Head { get { return Text.Substring(0, Start); } }

        /// <summary>
        /// The selected part of the string.
        /// </summary>
        public string Selection { get { return Text.Substring(Start, Length); } }

        /// <summary>
        /// The selected part of the string or the whole string if nothing is selected.
        /// </summary>
        public string EffectiveSelection { get { return Length == 0 ? Text : Selection; } }

        /// <summary>
        /// Part after selection.
        /// </summary>
        public string Tail { get { return Text.Substring(Start + Length); } }

        /// <summary>
        /// Replace selection with insert text.
        /// </summary>
        /// <param name="insert">The text to be inserted.</param>
        /// <returns>The resulting value.</returns>
        public TextSlice Insert(string insert)
        {
            var text = Head + insert + Tail;
            var result = new TextSlice(text, Start + insert.Length, 0, true);
            return result;
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="lhs">Left hand side.</param>
        /// <param name="rhs">Right hand side.</param>
        /// <returns>The start and length of the two sides match.</returns>
        public static bool operator ==(TextSlice lhs, TextSlice rhs)
        {
            return lhs.Text == rhs.Text && lhs.Start == rhs.Start && lhs.Length == rhs.Length && (lhs.Length == 0 || lhs.IsCaretAtEnd == rhs.IsCaretAtEnd);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="lhs">Left hand side.</param>
        /// <param name="rhs">Right hand side.</param>
        /// <returns>The start and length of the two sides do not match match.</returns>
        public static bool operator !=(TextSlice lhs, TextSlice rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Equality test.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>The two objects are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is TextSlice && this == (TextSlice)obj;
        }

        /// <summary>
        /// Get hash code for object.
        /// </summary>
        /// <returns>The hash.</returns>
        public override int GetHashCode()
        {
            return Text.GetHashCode() ^ Start.GetHashCode() ^ Length.GetHashCode();
        }

        /// <summary>
        /// Get string representation of object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            return $"\"{Text}\"[{Start}+{Length}]";
        }
    }
}
