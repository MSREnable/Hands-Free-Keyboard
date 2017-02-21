using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.HandsFree.Mouse;
using Microsoft.HandsFree.Settings;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.OnScreenKeyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IKeyboardHost
    {
        public MainWindow()
        {
            DataContext = this;

            Loaded += (s, e) =>
            {
                var mouseSettings = AppSettings.Instance.Mouse;
                mouseSettings.TrackActiveWindowOnly = false;
                GazeMouse.Attach(this, null, null, mouseSettings);
            };

            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        ToggleStateCollection IKeyboardHost.ToggleStates
        {
            get
            {
                return _toggleStates;
            }
        }
        ToggleStateCollection _toggleStates = new ToggleStateCollection();

        ICommand IKeyboardHost.GetAction(string name)
        {
            throw new NotImplementedException();
        }

        void IKeyboardHost.PlaySimpleKeyFeedback(string vocal)
        {
        }

        void IKeyboardHost.SendAlphanumericKeyPress(string key, string vocal)
        {
            SendKeys.SendWait(key);
        }

        void IKeyboardHost.SpeakFixedText(string text)
        {
        }

        void IKeyboardHost.ShowException(string context, Exception ex)
        {
            // Keep calm and carry on.
        }
    }
}
