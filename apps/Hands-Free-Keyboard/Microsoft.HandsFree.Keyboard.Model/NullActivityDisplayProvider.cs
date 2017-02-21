using Microsoft.HandsFree.Keyboard.Model;
using System;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Implementation of IActivityDisplayProvider.
    /// </summary>
    public class NullActivityDisplayProvider : IActivityDisplayProvider
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly IActivityDisplayProvider Instance = new NullActivityDisplayProvider();

        /// <summary>
        /// Private constructor.
        /// </summary>
        NullActivityDisplayProvider()
        {
        }

        bool IActivityDisplayProvider.IsOn { get; set; }

        bool IActivityDisplayProvider.IsTyping { get; set; }

        bool IActivityDisplayProvider.IsSpeaking { get; set; }

        string IActivityDisplayProvider.Status => string.Empty;

        event EventHandler IActivityDisplayProvider.StatusChanged { add { } remove { } }
    }
}
