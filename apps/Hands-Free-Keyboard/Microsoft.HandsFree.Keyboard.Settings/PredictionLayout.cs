using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// The way predictions are to be shown.
    /// </summary>
    public enum PredictionLayout
    {
        /// <summary>
        /// Only predicted words to be shown.
        /// </summary>
        [Description("Only show words")]
        WordsAlone,

        /// <summary>
        /// Only predicted phrase words to be shown.
        /// </summary>
        [Description("Only show phrase")]
        PhraseAlone,

        /// <summary>
        /// Active listening mode.
        /// </summary>
        [Description("Active listening")]
        ActiveListening,
    }
}
