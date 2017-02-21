using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Keyboard.Model.Test
{
    [TestClass]
    public class TextBoxEditorTest
    {
        const string Undo = "^Z";
        const string Redo = "^Y{END}";

        static void Check(TextBoxEditor editor, string keys, TextSlice expected)
        {
            var clipboardProvider = new TestClipboardProvider();
            editor.Interpret(keys);
            Assert.AreEqual(expected, editor.TextSlice, "Expected result of keystroke");
        }

        static void Check(TextBoxEditor editor, string keys, string expectedText, int expectedStart, int expectedLength)
        {
            var slice = new TextSlice(expectedText, expectedStart, expectedLength, true);
            Check(editor, keys, slice);
        }

        static void Check(TextBoxEditor editor, string keys, string expectedText)
        {
            Check(editor, keys, expectedText, expectedText.Length, 0);
        }

        [TestMethod]
        public void SimpleTyping()
        {
            var editor = new TextBoxEditor(new TestClipboardProvider());

            Assert.AreEqual(editor.TextSlice, TextSlice.Empty);
            Check(editor, "A", "A");
            Check(editor, "B", "AB");

            Check(editor, Undo, "A");
            Check(editor, Undo, "");
            Check(editor, Undo, "");

            Check(editor, Redo, "A");
            Check(editor, Redo, "AB");
            Check(editor, Redo, "AB");
        }

        [TestMethod]
        public void ClipboardAccess()
        {
            IClipboardProvider clipboardProvider = new TestClipboardProvider();
            var editor = new TextBoxEditor(clipboardProvider);

            Assert.IsNull(clipboardProvider.GetText());
            editor.Interpret("Hello World");
            editor.SelectAll();
            editor.Interpret("^C");
            Assert.AreEqual("Hello World", clipboardProvider.GetText());
            editor.Interpret("^V");
            editor.Interpret("^V");
            Assert.AreEqual("Hello WorldHello World", editor.TextSlice.Text);
        }
    }
}
