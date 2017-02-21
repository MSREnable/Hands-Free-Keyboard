using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    // TODO:
    // Is it ok to refer to keyboard layout names as Tobii
    /// <summary>
    /// The keyboard layout to use.
    /// </summary>
    public enum KeyboardLayoutName
    {
        /// <summary>
        /// The default layout.
        /// </summary>
        Default,

        /// <summary>
        /// Three rowed layout.
        /// </summary>
        ThreeRowed,

        /// <summary>
        /// Layout similar to that offered by Tobii.
        /// </summary>
        Tobii,

        /// <summary>
        /// Layout similar to the Windows on-screen pop-up keyboard.
        /// </summary>
        Tabtip,

        /// <summary>
        /// SteveK special.
        /// </summary>
        Krohn,

        /// <summary>
        /// 14 column interpretation of Arturo12.
        /// </summary>
        [Description("Non-Arturo 14")]
        NonArturo14,

        /// <summary>
        /// 10.5 column supersized keyboard.
        /// </summary>
        Supersize,

        /// <summary>
        /// Ambiguous keyboard layout with 15 keys
        /// </summary>
        Q15,

        /// <summary>
        /// Ambiguous keyboard layout with 9 keys
        /// </summary>
        Q9,

        /// <summary>
        /// Ambiguous keyboard layout with 6 keys
        /// </summary>
        Q6,

        /// <summary>
        /// Layout specified by Keyboard.xml in user's Documents directory.
        /// </summary>
        Custom,
    }
}
