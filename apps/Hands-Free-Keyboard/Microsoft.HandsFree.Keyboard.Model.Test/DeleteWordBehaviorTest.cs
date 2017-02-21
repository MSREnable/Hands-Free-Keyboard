using System;
using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Keyboard.Model.Test
{
    [TestClass]
    public class DeleteWordBehaviorTest : IClipboardProvider
    {
        string _clipboard;

        string IClipboardProvider.GetText()
        {
            return _clipboard;
        }

        void IClipboardProvider.SetText(string text)
        {
            _clipboard = text;
        }

        static TextSlice DoDeleteWord(TextSlice input)
        {
            var output = KeystrokeInterpreter.Interpret(null, input, "{DELWORD}");
            return output;
        }

        void CheckNonEmptyDelete(TextSlice input, TextSlice expected, string keystroke)
        {
            var actual = KeystrokeInterpreter.Interpret(this, input, keystroke);
            Assert.AreEqual(expected, actual, keystroke);
        }

        void Check(params string[] parts)
        {
            var compound = string.Join(string.Empty, parts);

            // Check simple delete behaviour.
            for (var start = 0; start < compound.Length; start++)
            {
                for (var end = start + 1; end < compound.Length; end++)
                {
                    var input = new TextSlice(compound, start, end - start, true);
                    var output = DoDeleteWord(input);

                    var expected = new TextSlice(compound.Substring(0, start) + compound.Substring(end), start, 0, true);
                    Assert.AreEqual(expected, output);

                    CheckNonEmptyDelete(input, expected, "{BACKSPACE}");
                    CheckNonEmptyDelete(input, expected, "{BKSP}");
                    CheckNonEmptyDelete(input, expected, "{BS}");
                    CheckNonEmptyDelete(input, expected, "{DELETE}");
                    CheckNonEmptyDelete(input, expected, "{DEL}");

                    _clipboard = null;
                    CheckNonEmptyDelete(input, expected, "^X");
                    Assert.AreEqual(input.Selection, ((IClipboardProvider)this).GetText());
                }
            }

            // Check actual word deletion behaviour.

            var wordIndex = 0;
            var wordStart = 0;
            var wordRemaining = parts[0].Length;
            var expectedSlice = new TextSlice(compound.Substring(wordRemaining), 0);
            for (var charIndex = 0; charIndex <= compound.Length; charIndex++)
            {
                var input = new TextSlice(compound, charIndex);

                var actualSlice = DoDeleteWord(input);

                Assert.AreEqual(expectedSlice, actualSlice);

                if (wordRemaining != 0)
                {
                    wordRemaining--;
                }
                else
                {
                    wordIndex++;
                    if (wordIndex < parts.Length)
                    {
                        wordStart = charIndex;
                        wordRemaining = parts[wordIndex].Length - 1;
                        var expectedText = string.Empty;
                        for (var i = 0; i < parts.Length; i++)
                        {
                            if (i != wordIndex)
                            {
                                expectedText += parts[i];
                            }
                        }
                        expectedSlice = new TextSlice(expectedText, charIndex);
                    }
                }
            }
        }

        [TestMethod]
        public void JustAlphas()
        {
            Check("", "Word");
        }

        [TestMethod]
        public void TheQuickBrownFox()
        {
            Check("", "The ", "quick ", "brown ", "fox");
        }

        [TestMethod]
        public void WordsWithWackySpaces()
        {
            Check("  ", "Hello  ", "World  ");
        }
    }
}
