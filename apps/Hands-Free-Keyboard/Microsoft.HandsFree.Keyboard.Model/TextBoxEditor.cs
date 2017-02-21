using Microsoft.HandsFree.Keyboard.Controls;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Editor for interpreting keystrokes sent to a text box.
    /// </summary>
    public class TextBoxEditor
    {
        const int MaxDosCount = 100;
        readonly List<TextSlice> _dos = new List<TextSlice>(MaxDosCount);
        int _doPosition;
        int _redoLimit;

        readonly IClipboardProvider _clipboardProvider;

        public TextBoxEditor(IClipboardProvider clipboardProvider)
        {
            _clipboardProvider = clipboardProvider;
        }

        /// <summary>
        /// The current text slice.
        /// </summary>
        public TextSlice TextSlice
        {
            get { return _textSlice; }
            set
            {
                if (value.Text != _textSlice.Text)
                {
                    if (_dos.Count == MaxDosCount)
                    {
                        _dos.RemoveAt(0);

                        if (0 < _doPosition)
                        {
                            _doPosition--;
                        }
                    }
                    _dos.Add(_textSlice);
                    _doPosition = _dos.Count;
                }
                _textSlice = value;
            }
        }
        TextSlice _textSlice = TextSlice.Empty;

        int Do(int position)
        {
            var slice = _dos[position];

            var newPosition = (slice.Text == _textSlice.Text || _dos.Count < MaxDosCount) ? position : position - 1;

            TextSlice = slice;

            return newPosition;
        }

        /// <summary>
        /// Interpret keystrokes.
        /// </summary>
        /// <param name="keys">SendKeys coded keystrokes.</param>
        public TextSlice Interpret(string keys)
        {
            switch (keys.ToUpperInvariant())
            {
                // Undo.
                case "^Z":
                    if (0 < _doPosition)
                    {
                        _doPosition = Do(_doPosition - 1);
                    }
                    break;

                // Redo.
                case "^Y":
                case "^Y{END}":
                    if (_doPosition < _redoLimit)
                    {
                        _doPosition = Do(_doPosition + 1);
                    }
                    break;

                default:
                    var slice = KeystrokeInterpreter.Interpret(_clipboardProvider, TextSlice, keys);
                    if (slice != TextSlice)
                    {
                        TextSlice = slice;
                        _redoLimit = _dos.Count;
                    }
                    break;
            }

            return TextSlice;
        }

        /// <summary>
        /// Select all the text.
        /// </summary>
        public TextSlice SelectAll()
        {
            TextSlice = new TextSlice(_textSlice.Text, 0, _textSlice.Text.Length, true);

            return TextSlice;
        }

        /// <summary>
        /// Reset text.
        /// </summary>
        public TextSlice Clear()
        {
            TextSlice = TextSlice.Empty;

            return TextSlice;
        }
    }
}
