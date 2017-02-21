using System;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Interface to outward facing display device.
    /// </summary>
    public interface IActivityDisplayProvider
    {
        /// <summary>
        /// Is the unit on.
        /// </summary>
        bool IsOn { get; set; }

        /// <summary>
        /// Is the user typing.
        /// </summary>
        bool IsTyping { get; set; }

        /// <summary>
        /// Is the user speaking.
        /// </summary>
        bool IsSpeaking { get; set; }

        string Status { get; }

        event EventHandler StatusChanged;
    }
}
