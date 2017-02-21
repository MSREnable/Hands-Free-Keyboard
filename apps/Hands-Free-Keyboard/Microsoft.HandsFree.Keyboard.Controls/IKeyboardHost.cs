using System;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Interface to be implenmented by key container window.
    /// </summary>
    public interface IKeyboardHost
    {
        /// <summary>
        /// Collection of toggle states.
        /// </summary>
        ToggleStateCollection ToggleStates { get; }

        /// <summary>
        /// Member to send a keystroke.
        /// </summary>
        /// <param name="key">The encoded key.</param>
        /// <param name="vocal">Vocal version of key.</param>
        void SendAlphanumericKeyPress(string key, string vocal);

        /// <summary>
        /// Perform the named keyboard action.
        /// </summary>
        /// <param name="name">Name of action to perform.</param>
        ICommand GetAction(string name);

        /// <summary>
        /// Produce vocalisation for a key.
        /// </summary>
        /// <param name="vocal">Vocalisation hint.</param>
        void PlaySimpleKeyFeedback(string vocal);

        /// <summary>
        /// Speak some fixed text.
        /// </summary>
        /// <param name="text">The text to speak.</param>
        void SpeakFixedText(string text);

        /// <summary>
        /// Show an otherwise unhandled exception.
        /// </summary>
        /// <param name="context">Description of context of exception.</param>
        /// <param name="ex">The exception to show.</param>
        void ShowException(string context, Exception ex);
    }
}
