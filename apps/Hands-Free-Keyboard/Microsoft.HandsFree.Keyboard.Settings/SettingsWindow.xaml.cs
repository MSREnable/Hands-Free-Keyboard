using Microsoft.HandsFree.Mouse;
using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings.Nudgers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private readonly GazeClickParameters defaultClickParams;
        private readonly GazeClickParameters calibrateClickParams;
        private readonly GazeClickParameters exitClickParams;

        /// <summary>
        /// 
        /// </summary>
        public SettingsWindow()
        {
            var onlyMouseUpDelay = (AppSettings.Instance.Keyboard.GazeClickDelay - GazeMouse.DefaultMouseDownDelay);
            var clickDelay = (uint)AppSettings.Instance.Keyboard.GazeClickDelay;
            var repeatMouseDownDelay = (uint)(clickDelay + onlyMouseUpDelay);

            defaultClickParams = new GazeClickParameters()
            {
                MouseDownDelay = GazeMouse.DefaultMouseDownDelay,
                MouseUpDelay = clickDelay,
                RepeatMouseDownDelay = repeatMouseDownDelay
            };

            calibrateClickParams = new GazeClickParameters
            {
                MouseDownDelay = GazeMouse.DefaultMouseDownDelay,
                MouseUpDelay = clickDelay,
                RepeatMouseDownDelay = uint.MaxValue
            };

            exitClickParams = new GazeClickParameters
            {
                MouseDownDelay = GazeMouse.DefaultMouseDownDelay,
                MouseUpDelay = uint.MaxValue,
                RepeatMouseDownDelay = uint.MaxValue
            };

            ResetSettingsCommand = new RelayCommand((p) => { AppSettings.Store.Reset(); });
            CloseSettingsCommand = new RelayCommand((p) => { AppSettings.Store.Save(); Close(); });

            InitializeComponent();

            Loaded += SettingsWindow_Loaded;
            Unloaded += (s, e) => Owner.IsEnabled = true;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Owner.IsEnabled = false;
            GazeMouse.Attach(this, null, GetGazeClickParameters, AppSettings.Instance.Mouse, true);
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ResetSettingsCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ICommand CloseSettingsCommand { get; }

        GazeClickParameters GetGazeClickParameters(FrameworkElement element)
        {
            if (element == ExitButton)
            {
                return exitClickParams;
            }
            else if (element == CalibrateButton)
            {
                return calibrateClickParams;
            }
            else
            {
                // TODO: This is a hack and should be replaced by a declarative XAML approach.
                var nudger = element.DataContext as ValueNudger;
                if (nudger?.Value is Sensors.Sensors)
                {
                    return exitClickParams;
                }

                return defaultClickParams;
            }
        }
    }
}
