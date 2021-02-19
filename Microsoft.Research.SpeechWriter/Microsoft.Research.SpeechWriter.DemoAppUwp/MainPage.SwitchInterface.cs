using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    public sealed partial class MainPage
    {
        private bool _switchMode;
        private int _switchClickCount;
        private DispatcherTimer _switchTimer = new DispatcherTimer();

        private void ShowSwitchInterface()
        {
            _switchMode = true;
            _switchClickCount = 0;
            _switchTimer.Interval = TimeSpan.FromSeconds(10);
            _switchTimer.Start();
        }

        private void OnSpace(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;

            _switchTimer.Stop();

            if (!_switchMode)
            {
                Debug.WriteLine("Enter switch mode");
                ShowSwitchInterface();
            }
            else
            {
                _switchClickCount++;
                _switchTimer.Interval = TimeSpan.FromSeconds(0.5);
                _switchTimer.Start();
            }
        }
        private void OnSwitchTimerTick(object sender, object e)
        {
            _switchTimer.Stop();
            if (_switchClickCount == 0)
            {
                Debug.WriteLine("Exit switch mode");
                _switchMode = false;
            }
            else
            {
                Debug.WriteLine($"Clicked {_switchClickCount} positions");
                ShowSwitchInterface();
            }
        }
    }
}
