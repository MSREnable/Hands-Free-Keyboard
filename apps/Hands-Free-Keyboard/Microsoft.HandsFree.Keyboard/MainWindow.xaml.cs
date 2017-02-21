using Microsoft.HandsFree.Helpers.Telemetry;
using Microsoft.HandsFree.Keyboard.ConcreteImplementations;
using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.MVVM;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard
{
    public sealed partial class MainWindow
    {
        private readonly KeyboardHost _host;
        private static readonly GeoCoordinateWatcher GeoWatcher = new GeoCoordinateWatcher();

        public MainWindow()
        {
            GeoWatcher.PositionChanged += GeoWatcher_PositionChanged;
            GeoWatcher.Start(false);

            ShowSettings = new RelayCommand(OnShowSettings);

            IKeyboardApplicationEnvironment environment = KeyboardApplicationEnvironment.Create(this);
            _host = environment.Host;

            InitializeComponent();

            EulaWindow.ShowDialogOnFirstRun();
        }

        private static void GeoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (!e.Position.Location.IsUnknown)
            {
                // Record only once
                GeoWatcher.Stop();

                new LocationMessage("Inventory")
                {
                    MessageId = IdMapper.GetId(InventoryMessages.Location),
                    HostName = Environment.MachineName,
                    UsernameSecure = Environment.UserName,
                    Location = new Tuple<double, double>(e.Position.Location.Latitude, e.Position.Location.Longitude)
                }.Enqueue();
            }
        }

        public ICommand ShowSettings { get; private set; }

        #region Window Event Handlers
        private void Window_Loaded(object sender, EventArgs e)
        {
            TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(EventId.AppStart));

            // New session
            Properties.Settings.Default.LastSessionId = Guid.Empty;
            Properties.Settings.Default.LastSessionStart = DateTime.Now;
            Properties.Settings.Default.LastSessionStop = DateTime.Now;
            Properties.Settings.Default.LastSessionLength = 0;
            Properties.Settings.Default.Save();

            // Try to initialize the ellipsis
            ActivityDisplayProvider.Instance.IsOn = true;

            // Give the textbox focus on launch
            TheTextBox.Focus();
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            AppSettings.Store.Save();

            var process = Process.GetCurrentProcess();
            var startTime = process.StartTime;
            var currentTime = DateTime.Now;
            var elapsedTime = currentTime - startTime;

            Properties.Settings.Default.LastSessionId = InstanceIdentityProvider.Instance.SessionGuid;
            Properties.Settings.Default.LastSessionStart = startTime;
            Properties.Settings.Default.LastSessionStop = currentTime;
            Properties.Settings.Default.LastSessionLength = elapsedTime.Ticks;
            Properties.Settings.Default.Save();

            // Turn off ellipsis
            ActivityDisplayProvider.Instance.IsOn = false;

            TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(EventId.AppStop));
            foreach (TraceListener listener in TelemetryMessage.Telemetry.Listeners)
            {
                listener.Flush();
                listener.Dispose();
            }
        }

        #endregion

        void OnShowSettings(object o)
        {
            var settingsWindow = new SettingsWindow()
            {
                Owner = this,
                DataContext = _host
            };
            settingsWindow.ShowDialog();
        }
    }
}
