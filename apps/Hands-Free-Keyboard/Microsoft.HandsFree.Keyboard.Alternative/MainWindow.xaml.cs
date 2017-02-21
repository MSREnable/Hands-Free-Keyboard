using Microsoft.HandsFree.Keyboard.ConcreteImplementations;
using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.Alternative
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IKeyboardApplicationEnvironment _environment;

        readonly KeyboardHost _host;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            _environment = KeyboardApplicationEnvironment.Create(this);
            _host = _environment.Host;

            InitializeComponent();
        }

        #region Window Event Handlers
        private void Window_Loaded(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.LastSessionId != Guid.Empty)
            {
                // Log the previous entry
                //TelemetryUtility.Log(new ApplicationEvent()
                //{
                //    AppName = InstanceIdentityProvider.Instance.AppName,
                //    Event = ApplicationEventType.Stop,
                //    UserId = InstanceIdentityProvider.Instance.UserId,
                //    SessionId = Properties.Settings.Default.LastSessionId,
                //    Duration = Properties.Settings.Default.LastSessionLength,
                //});

                // Clear it out so it doesn't get double logged in the case of a crash
                Properties.Settings.Default.LastSessionId = Guid.Empty;
                Properties.Settings.Default.LastSessionLength = 0;
                Properties.Settings.Default.Save();
            }

            // Try to initialize the ellipsis
            ActivityDisplayProvider.Instance.IsOn = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var process = Process.GetCurrentProcess();
            var startTime = process.StartTime;
            var currentTime = DateTime.Now;
            var elapsedTime = currentTime - startTime;

            Properties.Settings.Default.LastSessionId = InstanceIdentityProvider.Instance.SessionGuid;
            Properties.Settings.Default.LastSessionLength = elapsedTime.Ticks;
            Properties.Settings.Default.Save();

            // Turn off ellipsis
            ActivityDisplayProvider.Instance.IsOn = false;
        }

        #endregion
    }
}
