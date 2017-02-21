using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Keyboard.Model.Test
{
    [TestClass]
    public class KeystrokeCasesTest
    {
        static readonly Dictionary<string, TextSlice> strokeToSlice = new Dictionary<string, TextSlice>();

        static readonly TextSlice Before = new TextSlice("AB CD EF", 4, 0, true);

        static void OutOfScope(string keystroke)
        {
            strokeToSlice.Add(keystroke, null);
        }

        static void Simple(string keystroke)
        {
            var escapedKeystroke = KeyboardHost.AutoEscape(keystroke);
            strokeToSlice.Add(escapedKeystroke, new TextSlice("AB C" + keystroke + "D EF", Before.Start + keystroke.Length, 0, true));
        }

        static void Explicit(string keystroke, TextSlice after)
        {
            strokeToSlice.Add(keystroke, after);
        }

        static void Explicit(string keystroke, string text, int start, int length, bool isCaretAtEnd)
        {
            Explicit(keystroke, new TextSlice(text, start, length, isCaretAtEnd));
        }

        static void Explicit(string keystroke, string text, int start, int length)
        {
            Explicit(keystroke, text, start, length, true);
        }

        static void Explicit(string keystroke, string text, int start)
        {
            Explicit(keystroke, text, start, 0);
        }

        static void UnrecognizedFunction(string keystroke)
        {
            Assert.AreEqual('{', keystroke[0]);
            Assert.AreEqual('}', keystroke[keystroke.Length - 1]);
            Explicit(keystroke, "AB C" + keystroke.Substring(1, keystroke.Length - 2) + "D EF", 4 + keystroke.Length - 2);
        }

        static void UnimplementedFunction(string keystroke)
        {
            Explicit(keystroke, Before);
        }

        static KeystrokeCasesTest()
        {
            Simple("'");
            Simple("-");
            Simple(" ");
            Simple("!");
            Simple("\"");
            Simple("#");
            Simple("$");
            Simple("%");
            Simple("&");
            Simple("(");
            Simple(")");
            Simple("*");
            Simple(",");
            Simple(", ");
            Simple(".");
            Simple(". ");
            Simple("/");
            Simple(":");
            Simple(";");
            Simple("?");
            Simple("@");
            Simple("[");
            Simple("\\");
            Simple("]");
            Simple("^");
            OutOfScope("^y{END}");
            OutOfScope("^Y{END}");
            OutOfScope("^z");
            OutOfScope("^Z");
            Simple("_");
            Explicit("{{}", "AB C{D EF", 5);
            Explicit("{BACKSPACE}", "AB D EF", 3);
            Explicit("{DEL}", "AB C EF", 4);
            Explicit("{DELWORD}", "AB EF", 3);
            UnimplementedFunction("{DOWN}");
            Explicit("{END}", "AB CD EF", 8);
            Explicit("{ENTER}", "AB C\nD EF", 5);
            UnimplementedFunction("{ESC}");
            UnimplementedFunction("{F1}");
            UnimplementedFunction("{F10}");
            UnimplementedFunction("{F11}");
            UnimplementedFunction("{F12}");
            UnimplementedFunction("{F2}");
            UnimplementedFunction("{F3}");
            UnimplementedFunction("{F4}");
            UnimplementedFunction("{F5}");
            UnimplementedFunction("{F6}");
            UnimplementedFunction("{F7}");
            UnimplementedFunction("{F8}");
            UnimplementedFunction("{F9}");
            Explicit("{HOME}", "AB CD EF", 0);
            UnimplementedFunction("{INS}");
            Explicit("{LEFT}", "AB CD EF", 3);
            UnimplementedFunction("{PGDN}");
            UnimplementedFunction("{PGUP}");
            UnimplementedFunction("{PRTSC}");
            Explicit("{RIGHT}", "AB CD EF", 5);
            Explicit("{TAB}", "AB C\tD EF", 5);
            UnimplementedFunction("{UP}");
            Simple("|");
            Simple("}");
            Simple("~");
            Simple("¦");
            Simple("£");
            Simple("€");
            Simple("+");
            OutOfScope("+^{LEFT}{DEL}");
            Explicit("+{END}", "AB CD EF", 8, 0);
            Explicit("+{HOME}", "AB CD EF", 0, 0, false);
            Explicit("+{LEFT}", "AB CD EF", 3, 0, false);
            Explicit("+{RIGHT}", "AB CD EF", 5, 0);
            Simple("<");
            Simple("=");
            Simple(">");
            Simple("©");
            Simple("°");
            Simple("µ");
            Simple("¶");
            Simple("•");
            Simple("0");
            Simple("½");
            Simple("1");
            Simple("2");
            Simple("3");
            Simple("4");
            Simple("5");
            Simple("6");
            Simple("7");
            Simple("8");
            Simple("9");
            Simple("a");
            Simple("A");
            Simple("b");
            Simple("B");
            Simple("c");
            Simple("C");
            Simple("d");
            Simple("D");
            Simple("e");
            Simple("E");
            Simple("f");
            Simple("F");
            Simple("g");
            Simple("G");
            Simple("h");
            Simple("H");
            Simple("i");
            Simple("I");
            Simple("j");
            Simple("J");
            Simple("k");
            Simple("K");
            Simple("l");
            Simple("L");
            Simple("m");
            Simple("M");
            Simple("n");
            Simple("N");
            Simple("o");
            Simple("O");
            Simple("p");
            Simple("P");
            Simple("q");
            Simple("Q");
            Simple("r");
            Simple("R");
            Simple("s");
            Simple("S");
            Simple("t");
            Simple("T");
            Simple("u");
            Simple("U");
            Simple("v");
            Simple("V");
            Simple("w");
            Simple("W");
            Simple("x");
            Simple("X");
            Simple("y");
            Simple("Y");
            Simple("z");
            Simple("Z");
            Simple("😁");
            Simple("😂");
            Simple("😃");
            Simple("😄");
            Simple("😅");
            Simple("😆");
            Simple("😇");
            Simple("😈");
            Simple("😉");
            Simple("😊");
            Simple("😋");
            Simple("😌");
            Simple("😍");
            Simple("😎");
            Simple("😏");
            Simple("😐");
            Simple("😒");
            Simple("😓");
            Simple("😔");
            Simple("😖");
            Simple("😘");
            Simple("😚");
            Simple("😜");
            Simple("😝");
            Simple("😞");
            Simple("😠");
            Simple("😡");
            Simple("😢");
            Simple("😣");
            Simple("😤");

            UnrecognizedFunction("{NOTHISISNOTAREALFUNCTION}");
        }

        internal static void AssertEverything(SortedSet<string> keystrokes)
        {
            var unseen = new SortedSet<string>(keystrokes);

            unseen.RemoveWhere((s) => strokeToSlice.ContainsKey(s));

            Assert.AreEqual(0, unseen.Count);
        }

        internal static void CheckCovered(string keystrokeValue)
        {
            var escapedKeystrokeValue = KeyboardHost.AutoEscape(keystrokeValue);
            Assert.IsTrue(strokeToSlice.ContainsKey(escapedKeystrokeValue), "Tested keystroke");
        }

        [TestMethod]
        public void KeystrokeCases()
        {
            foreach (var pair in strokeToSlice)
            {
                var expected = pair.Value;
                var actual = KeystrokeInterpreter.Interpret(null, Before, pair.Key);

                if (!ReferenceEquals(expected, null))
                {
                    //if (actual != expected)
                    //{
                    //    KeystrokeInterpreter.Interpret(null, Before, pair.Key);
                    //}

                    Assert.AreEqual(expected, actual);
                }
            }
        }
    }
}
