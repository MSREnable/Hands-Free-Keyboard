using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Baked in vocalisation themes.
    /// </summary>
    public enum NarrationTheme
    {
        /// <summary>
        /// Non standard theme.
        /// </summary>
        Custom,

        /// <summary>
        /// Original simplely voicing whole utterances scheme.
        /// </summary>
        Original,

        /// <summary>
        /// Audio features demonstration.
        /// </summary>
        [Description("Demonstration Mode")]
        Demo,

        /// <summary>
        /// Feedback scheme, reading key-by-key and word-by-word but not announcing completed sentences.
        /// </summary>
        [Description("Private Feedback")]
        PrivateFeedback,

        /// <summary>
        /// Public scheme reading word-by-word and repeated sentences.
        /// </summary>
        [Description("Simple Output")]
        SimplePublic,

        /// <summary>
        /// Simple public scheme extended with filling echo.
        /// </summary>
        [Description("Echoing Output")]
        EchoFilledPublic,
    }
}
