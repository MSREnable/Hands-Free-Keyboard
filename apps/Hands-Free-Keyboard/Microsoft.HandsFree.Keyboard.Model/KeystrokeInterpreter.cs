using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.HandsFree.Prediction.Api;
using System;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Implementation of TextBox+SendKeys logic.
    /// </summary>
    public static class KeystrokeInterpreter
    {
        static void Failed(string message)
        {
            // Breakpoint for logic failure.
        }

        static void Assertion(bool isPassed, string message)

        {
            if (!isPassed)
            {
                Failed(message);
            }
        }

        /// <summary>
        /// Interpret keystrokes.
        /// </summary>
        /// <param name="clipboardProvider">The clipboard abstraction</param>
        /// <param name="slice">The slice.</param>
        /// <param name="keys">SendKeys coded keystrokes.</param>
        public static TextSlice Interpret(IClipboardProvider clipboardProvider, TextSlice slice, string keys)
        {
            switch (keys.ToUpperInvariant())
            {
                case "+^{LEFT}{DEL}":
                    slice = Interpret(clipboardProvider, slice, "{DELWORD}");
                    break;

                default:
                    if (keys != "+^{LEFT}{DEL}")
                    {
                        // Not special case of Delete Word.
                        var position = 0;
                        var length = keys.Length;

                        var isShift = false;
                        var isControl = false;
                        var isAlt = false;

                        var isIntroducer = true;
                        while (isIntroducer && position < length)
                        {
                            var ch = keys[position];

                            switch (ch)
                            {
                                case '+':
                                    Assertion(!isShift, "Double shift");
                                    isShift = true;
                                    break;

                                case '^':
                                    Assertion(!isControl, "Double control");
                                    isControl = true;
                                    break;

                                case '%':
                                    Assertion(!isAlt, "Double alt");
                                    isAlt = true;
                                    break;

                                default:
                                    isIntroducer = false;
                                    break;
                            }

                            if (isIntroducer)
                            {
                                position++;
                            }
                        }

                        while (position < length)
                        {
                            var ch = keys[position];
                            var start = position;
                            position++;

                            if (isControl)
                            {
                                switch (char.ToUpperInvariant(ch))
                                {
                                    // Copy.
                                    case 'C':
                                        clipboardProvider.SetText(slice.EffectiveSelection);
                                        break;

                                    // Cut.
                                    case 'X':
                                        clipboardProvider.SetText(slice.EffectiveSelection);
                                        slice = new TextSlice(slice.Head + slice.Tail, slice.Start);
                                        break;

                                    // Paste.
                                    case 'V':
                                        var text = clipboardProvider.GetText();
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            slice = slice.Insert(text);
                                        }
                                        break;

                                    default:
                                        Failed("Unknown control");
                                        break;
                                }
                            }
                            else if (ch != '{')
                            {
                                while (position < length && keys[position] != '{')
                                {
                                    position++;
                                }

                                var insert = keys.Substring(start, position - start);
                                slice = slice.Insert(insert);
                            }
                            else
                            {
                                var end = keys.IndexOf('}', position + 1);
                                if (end == -1)
                                {
                                    Failed("Unclosed group");
                                }
                                else if (end == position)
                                {
                                    position++;
                                    if (position < length && keys[position] == '}')
                                    {
                                        var text = slice.Head + '{' + slice.Tail;
                                        slice = new TextSlice(text, slice.Start + 1);
                                    }
                                    else
                                    {
                                        Failed("Bad escape");
                                    }
                                }
                                else
                                {
                                    var keyword = keys.Substring(position, end - position).ToUpperInvariant();
                                    position = end + 1;

                                    var charSpan = 1;
                                    FunctionKey functionKey;
                                    if (Enum.TryParse<FunctionKey>(keyword, out functionKey))
                                    {
                                        switch (functionKey)
                                        {
                                            case FunctionKey.BACKSPACE:
                                                if (slice.Length != 0)
                                                {
                                                    slice = new TextSlice(slice.Head + slice.Tail, slice.Start);
                                                }
                                                else if (slice.Start != 0)
                                                {
                                                    if (slice.Start > 1 && char.IsSurrogatePair(slice.Text, slice.Start - 2))
                                                    {
                                                        charSpan = 2;
                                                    }

                                                    slice = new TextSlice(slice.Text.Substring(0, slice.Start - charSpan) + slice.Tail, slice.Start - charSpan);
                                                }
                                                break;

                                            case FunctionKey.DELETE:
                                                if (slice.Length != 0)
                                                {
                                                    slice = new TextSlice(slice.Head + slice.Tail, slice.Start);
                                                }
                                                else if (slice.Start != slice.Text.Length)
                                                {
                                                    if (slice.Text.Length - slice.Start > 1 && char.IsSurrogatePair(slice.Text, slice.Start))
                                                    {
                                                        charSpan = 2;
                                                    }

                                                    slice = new TextSlice(slice.Head + slice.Text.Substring(slice.Start + charSpan), slice.Start);
                                                }
                                                break;

                                            case FunctionKey.END:
                                                slice = new TextSlice(slice.Text, slice.Text.Length);
                                                break;

                                            case FunctionKey.ENTER:
                                                slice = slice.Insert("\n");
                                                break;

                                            case FunctionKey.HOME:
                                                slice = new TextSlice(slice.Text, 0);
                                                break;

                                            case FunctionKey.LEFT:
                                                if (slice.Start > 1 && char.IsSurrogatePair(slice.Text, slice.Start - 2))
                                                {
                                                    charSpan = 2;
                                                }

                                                if (slice.Length != 0)
                                                {
                                                    // Non-empty selection, move to start of selection.
                                                    slice = new TextSlice(slice.Text, slice.Start);
                                                }
                                                else if (0 < slice.Start)
                                                {
                                                    // Empty selection and not at start of text.
                                                    slice = new TextSlice(slice.Text, slice.Start - charSpan);
                                                }
                                                break;

                                            case FunctionKey.RIGHT:
                                                if (slice.Text.Length - slice.Start - slice.Length > 1 && char.IsSurrogatePair(slice.Text, slice.Start + slice.Length))
                                                {
                                                    charSpan = 2;
                                                }

                                                if (slice.Length != 0)
                                                {
                                                    // Non-empty selection, move to end.
                                                    slice = new TextSlice(slice.Text, slice.Start + slice.Length);
                                                }
                                                else if (slice.Start < slice.Text.Length)
                                                {
                                                    // Empty selection, move it right.
                                                    slice = new TextSlice(slice.Text, slice.Start + charSpan);
                                                }
                                                break;

                                            case FunctionKey.TAB:
                                                slice = slice.Insert("\t");
                                                break;

                                            case FunctionKey.DELWORD:
                                                if (slice.Length != 0)
                                                {
                                                    slice = new TextSlice(slice.Head + slice.Tail, slice.Start);
                                                }
                                                else
                                                {
                                                    int wordStart;
                                                    int punctuationEnd;

                                                    var sliceText = slice.Text;
                                                    var anchor = slice.Start;
                                                    var punctuationStart = anchor - sliceText.ReversePunctuationLength(anchor);
                                                    if (punctuationStart == 0 || punctuationStart != anchor)
                                                    {
                                                        // At start of next word.
                                                        punctuationEnd = anchor + sliceText.PunctuationLength(anchor);
                                                        wordStart = punctuationStart - sliceText.ReverseWordLength(punctuationStart);
                                                    }
                                                    else
                                                    {
                                                        // Within word.
                                                        wordStart = anchor - sliceText.ReverseWordLength(anchor);
                                                        var wordEnd = anchor + sliceText.WordLength(anchor);
                                                        punctuationEnd = wordEnd + sliceText.PunctuationLength(wordEnd);
                                                    }

                                                    slice = new TextSlice(sliceText.Substring(0, wordStart) + sliceText.Substring(punctuationEnd), wordStart);
                                                }
                                                break;

                                            default:
                                                Failed("Unimplemented escape");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        slice = slice.Insert(keyword);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return slice;
        }
    }
}
