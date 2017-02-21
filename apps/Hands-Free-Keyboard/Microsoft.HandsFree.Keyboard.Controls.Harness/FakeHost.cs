using System;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.Controls.Harness
{
    class FakeHost : IKeyboardHost
    {
        ToggleStateCollection IKeyboardHost.ToggleStates
        {
            get { throw new NotImplementedException(); }
        }

        void IKeyboardHost.SendAlphanumericKeyPress(string key, string vocal)
        {
            throw new NotImplementedException();
        }

        ICommand IKeyboardHost.GetAction(string name)
        {
            throw new NotImplementedException();
        }

        void IKeyboardHost.PlaySimpleKeyFeedback(string vocal)
        {
            throw new NotImplementedException();
        }

        void IKeyboardHost.SpeakFixedText(string text)
        {
            throw new NotImplementedException();
        }

        void IKeyboardHost.ShowException(string context, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
