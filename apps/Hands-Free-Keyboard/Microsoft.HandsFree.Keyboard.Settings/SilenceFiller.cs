using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Ways of filling silence.
    /// </summary>
    public enum SilenceFiller
    {
        /// <summary>
        /// Make no attempt to fill silence.
        /// </summary>
        [Description("None")]
        None,

        /// <summary>
        /// Play music to cover gaps.
        /// </summary>
        [Description("Play music")]
        Music,

        /// <summary>
        /// Introduce artificial echo of last fragment.
        /// </summary>
        [Description("Echoing")]
        Echo,

        /// <summary>
        /// Fill silence by muttering random words.
        /// </summary>
        [Description("Muttering")]
        Muttering
    }
}
