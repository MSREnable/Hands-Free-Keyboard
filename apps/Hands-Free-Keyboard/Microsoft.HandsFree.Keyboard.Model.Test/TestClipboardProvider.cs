using System;

namespace Microsoft.HandsFree.Keyboard.Model.Test
{
    internal class TestClipboardProvider : IClipboardProvider
    {
        string _text;

        TestClipboardProvider(string text)
        {
            _text = text;
        }

        internal TestClipboardProvider()
            :this(null)
        {
        }

        string IClipboardProvider.GetText()
        {
            return _text;
        }

        void IClipboardProvider.SetText(string text)
        {
            _text = text;
        }
    }
}
