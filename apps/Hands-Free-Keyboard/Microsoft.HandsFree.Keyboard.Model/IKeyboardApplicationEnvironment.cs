using Microsoft.HandsFree.Keyboard.Settings;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Environment that hosts keyboard application.
    /// </summary>
    public interface IKeyboardApplicationEnvironment
    {
        /// <summary>
        /// The KeyboardHost this environment is associated with.
        /// </summary>
        KeyboardHost Host { get; }

        /// <summary>
        /// Source of randomness.
        /// </summary>
        Random Random { get; }

        /// <summary>
        /// The instance identity provider.
        /// </summary>
        IInstanceIdentityProvider InstanceIdentityProvider { get; }

        /// <summary>
        /// The gaze provider.
        /// </summary>
        IGazeProvider GazeProvider { get; }

        /// <summary>
        /// Get the text to audio provider factory.
        /// </summary>
        ITextToAudioProviderFactory TextToAudioProviderFactory { get; }

        /// <summary>
        /// Get the audio provider factory.
        /// </summary>
        IAudioProviderFactory AudioProviderFactory { get; }

        /// <summary>
        /// The activity display provider.
        /// </summary>
        IActivityDisplayProvider ActivityDisplayProvider { get; }

        /// <summary>
        /// The system clipboard abstraction.
        /// </summary>
        IClipboardProvider ClipboardProvider { get; }

        /// <summary>
        /// The settings values.
        /// </summary>
        AppSettings AppSettings { get; }

        /// <summary>
        /// The system access provider.
        /// </summary>
        ISystemProvider SystemProvider { get; }

        /// <summary>
        /// Thread dispatcher.
        /// </summary>
        Dispatcher Dispatcher { get; }

        /// <summary>
        /// Is an update available.
        /// </summary>
        bool IsUpdateAvailable { get; }

        /// <summary>
        /// Event fired when update becomes available.
        /// </summary>
        event EventHandler UpdateAvailable;

        /// <summary>
        /// Restart the application with the available update.
        /// </summary>
        void RestartWithUpdate();

        /// <summary>
        /// Close and exit application.
        /// </summary>
        /// <remarks>
        /// Caller should not expect this function to return but should also not perform any further actions if it does.
        /// </remarks>
        void ExitApplication();

        /// <summary>
        /// Ask the user a Yes/No question.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <returns>True iff the user answers Yes.</returns>
        bool AskYesNo(string question);
    }
}
