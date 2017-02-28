using Microsoft.HandsFree.Helpers.Telemetry;
using Microsoft.HandsFree.Settings.Serialization;
using Splat;
using Squirrel;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.HandsFree.Helpers.Updates
{
    /// <summary>
    /// Class for managing application updates.
    /// </summary>
    public class UpdateHost
    {
        const ShortcutLocation NormalShortcutLocations = ShortcutLocation.Desktop | ShortcutLocation.StartMenu;
        const ShortcutLocation AutoStartShortcutLocations = NormalShortcutLocations | ShortcutLocation.Startup;
        ShortcutLocation ShortcutLocations => _isAutoStart ? AutoStartShortcutLocations : NormalShortcutLocations;

        /// <summary>
        /// Should application be configured to autostart at login.
        /// </summary>
        bool _isAutoStart;

        /// <summary>
        /// How frequently should the server be polled for an update.
        /// </summary>
        TimeSpan _pollInterval;

        /// <summary>
        /// Has the Application.Current.CloseApplication hook been placed?
        /// </summary>
        bool _isExitHookInstalled;

        static UpdateHost _instance;

        static Assembly EntryAssembly = Assembly.GetEntryAssembly();

        static AssemblyInformationalVersionAttribute AssemblyInformationalVersion = (AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(EntryAssembly, typeof(AssemblyInformationalVersionAttribute));

        static string AssemblyName = EntryAssembly.FullName.Split(new[] { ',' }, 2)[0];

        static string ProductName = AssemblyName.Substring(AssemblyName.LastIndexOf('.') + 1);

        static string Branch = AssemblyInformationalVersion.InformationalVersion;

        static string UpdateUrl = $"https://msrenable.blob.core.windows.net/install/Hands-Free-{ProductName}-{Branch}/latest/{AssemblyName}";

        UpdateManager _updateManager = new UpdateManager(UpdateUrl);

        UpdateHost(bool isAutoStart, TimeSpan pollInterval)
        {
            _isAutoStart = isAutoStart;
            _pollInterval = pollInterval;
        }

        /// <summary>
        /// Event fired when update becomes available.
        /// </summary>
        public static event EventHandler UpdateAvailable
        {
            add { _updateAvailable += value; }
            remove { _updateAvailable -= value; }
        }
        static event EventHandler _updateAvailable;

        /// <summary>
        /// Is an update available.
        /// </summary>
        public static bool IsUpdateAvailable { get; private set; }

        /// <summary>
        /// The available update.
        /// </summary>
        public static Version AvailableUpdate { get; private set; }

        private static bool IsPrivateBuild()
        {
            return Branch == "private";
        }

        void CreateShortcutForThisExe(Version version)
        {
            TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(UpdateTelemetryAction.CreateShortcut), version.ToString());
            foreach (TraceListener listener in TelemetryMessage.Telemetry.Listeners)
            {
                listener.Flush();
            }
            _updateManager.CreateShortcutsForExecutable(Path.GetFileName(
                EntryAssembly.Location),
                ShortcutLocations,
                Environment.CommandLine.Contains("squirrel-install") == false,
                null, null);
        }

        void RemoveShortcutForThisExe(Version version)
        {
            TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(UpdateTelemetryAction.RemoveShortcut), version.ToString());
            foreach (TraceListener listener in TelemetryMessage.Telemetry.Listeners)
            {
                listener.Flush();
            }
            _updateManager.RemoveShortcutsForExecutable(
                Path.GetFileName(EntryAssembly.Location),
                ShortcutLocations);
        }

        async Task UpdateThread()
        {
            Locator.CurrentMutable.Register(() => new SetupLogger(false) { Level = LogLevel.Info }, typeof(ILogger));

            do
            {
                ReleaseEntry entry;
                try
                {
                    entry = await _updateManager.UpdateApp();
                }
                catch (Exception)
                {
                    entry = null;
                }

                if (!_isExitHookInstalled)
                {
                    Application.Current.Dispatcher.Invoke(InstallHook);
                }

                if (entry == null)
                {
                    await Task.Delay(_pollInterval);
                    TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(UpdateTelemetryAction.PolledNoUpdate));
                }
                else
                {
                    TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(UpdateTelemetryAction.PollFoundUpate), entry.Version.ToString());

                    AvailableUpdate = entry.Version.Version;
                    IsUpdateAvailable = true;
                    _updateAvailable?.Invoke(this, EventArgs.Empty);

                    Stop();
                }
            }
            while (!IsUpdateAvailable && _pollInterval != TimeSpan.Zero);
        }

        void InstallHook()
        {
            if (!_isExitHookInstalled && Application.Current != null)
            {
                _isExitHookInstalled = true;
                Application.Current.Exit += (s, e) => Stop();
            }
        }

        void Initialize()
        {
            InstallHook();

            // Note, in most of these scenarios, the app exits after this method
            // completes!
            SquirrelAwareApp.HandleEvents(
              onInitialInstall: CreateShortcutForThisExe,
              onAppUpdate: CreateShortcutForThisExe,
              onAppUninstall: RemoveShortcutForThisExe);
            //onFirstRun: () => ShowTheWelcomeWizard = true);
        }

        /// <summary>
        /// Create the UpdateHost instance.
        /// </summary>
        public static void Start(bool isAutoStart, TimeSpan pollInterval)
        {
            if (!IsPrivateBuild())
            {
                _instance = new UpdateHost(isAutoStart, pollInterval);
                _instance.Initialize();

                var task = _instance.UpdateThread();
            }
        }

        /// <summary>
        /// Stop updating tracking.
        /// </summary>
        static void Stop()
        {
            if (_instance != null && _instance._updateManager != null)
            {
                var manager = _instance._updateManager;
                _instance._updateManager = null;
                manager.Dispose();
            }
        }

        /// <summary>
        /// Restart the application with the update applied.
        /// </summary>
        public static void Restart()
        {
            UpdateManager.RestartApp();
        }
    }
}
