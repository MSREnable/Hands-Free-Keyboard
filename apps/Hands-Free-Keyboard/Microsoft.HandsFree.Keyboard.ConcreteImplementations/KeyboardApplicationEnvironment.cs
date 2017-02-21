using Microsoft.HandsFree.Helpers.Updates;
using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Mouse;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    /// <summary>
    /// Concrete implementation of IKeyboardApplicationEnvironment.
    /// </summary>
    public class KeyboardApplicationEnvironment : IKeyboardApplicationEnvironment, IClipboardProvider
    {
        readonly Window _window;

        readonly GazeMouse.GetGazeClickParameters _getGazeClickParameters;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="window">The main window of the application.</param>
        /// <param name="getGazeClickParameters">The gaze click parameter provider.</param>
        KeyboardApplicationEnvironment(Window window, GazeMouse.GetGazeClickParameters getGazeClickParameters)
        {
            _window = window;
            _getGazeClickParameters = getGazeClickParameters;

            _window.Loaded += OnWindowLoaded;
            _window.Closed += (s, e) =>
              {
                  GazeMouse.DetachAll();
                  Environment.Exit(0);
              };

            AudioProviderFactory.UpdateSettings();
            TextToAudioProviderFactory.UpdateSettings();
        }

        /// <summary>
        /// The keyboard host created for this environment.
        /// </summary>
        public KeyboardHost Host { get; private set; }

        Random IKeyboardApplicationEnvironment.Random { get; } = new Random();

        IInstanceIdentityProvider IKeyboardApplicationEnvironment.InstanceIdentityProvider => InstanceIdentityProvider.Instance;

        IGazeProvider IKeyboardApplicationEnvironment.GazeProvider => _gazeProvider;
        IGazeProvider _gazeProvider;

        ITextToAudioProviderFactory IKeyboardApplicationEnvironment.TextToAudioProviderFactory { get { return TextToAudioProviderFactory.Instance; } }

        IAudioProviderFactory IKeyboardApplicationEnvironment.AudioProviderFactory { get { return AudioProviderFactory.Instance; } }

        IActivityDisplayProvider IKeyboardApplicationEnvironment.ActivityDisplayProvider { get { return ActivityDisplayProvider.Instance; } }

        ISystemProvider IKeyboardApplicationEnvironment.SystemProvider { get { return SystemProvider.Instance; } }

        AppSettings IKeyboardApplicationEnvironment.AppSettings { get { return AppSettings.Instance; } }

        Dispatcher IKeyboardApplicationEnvironment.Dispatcher { get { return _window.Dispatcher; } }

        bool IKeyboardApplicationEnvironment.IsUpdateAvailable { get { return UpdateHost.IsUpdateAvailable; } }

        event EventHandler IKeyboardApplicationEnvironment.UpdateAvailable
        {
            add { UpdateHost.UpdateAvailable += value; }
            remove { UpdateHost.UpdateAvailable -= value; }
        }

        void IKeyboardApplicationEnvironment.RestartWithUpdate() { UpdateHost.Restart(); }

        IClipboardProvider IKeyboardApplicationEnvironment.ClipboardProvider { get { return this; } }

        /// <summary>
        /// Create a new environment and associate it with a new host.
        /// </summary>
        /// <param name="window">The window to attach to.</param>
        /// <param name="getGazeClickParameters">The gaze click parameter provider.</param>
        /// <returns>The new environment.</returns>
        public static KeyboardApplicationEnvironment Create(Window window, GazeMouse.GetGazeClickParameters getGazeClickParameters)
        {
            var environment = new KeyboardApplicationEnvironment(window, getGazeClickParameters);
            var host = KeyboardHost.Create(environment);
            environment.Host = host;
            window.DataContext = host;

            return environment;
        }

        /// <summary>
        /// Create a new environment and associate it with a new host.
        /// </summary>
        /// <param name="window">The window to attach to.</param>
        /// <returns>The new environment.</returns>
        public static KeyboardApplicationEnvironment Create(Window window)
        {
            var environment = Create(window, null);
            return environment;
        }

        void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _gazeProvider = GazeProvider.Create(_window, _getGazeClickParameters);
        }

        void IKeyboardApplicationEnvironment.ExitApplication()
        {
            _window.Close();
        }

        bool IKeyboardApplicationEnvironment.AskYesNo(string question)
        {
            var result = HandsFreeMessageBox.ShowMessage(_window, question);
            return result;
        }

        string IClipboardProvider.GetText()
        {
            return Clipboard.GetText();
        }

        void IClipboardProvider.SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
}
