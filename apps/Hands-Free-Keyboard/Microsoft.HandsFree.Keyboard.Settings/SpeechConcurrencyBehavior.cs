using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Behavior to use when Play is pressed to play a sentence while a previous sentence is currently being spoken.
    /// </summary>
    public enum SpeechConcurrencyBehavior
    {
        /// <summary>
        /// The new sentence is spoken afresh.
        /// </summary>
        [Description("Always speak")]
        AlwaysSay,

        /// <summary>
        /// If the sentence is the same as the one being spoken, it is not spoken again.
        /// </summary>
        [Description("Suppress duplicates")]
        SuppressDuplicates
    }
}
