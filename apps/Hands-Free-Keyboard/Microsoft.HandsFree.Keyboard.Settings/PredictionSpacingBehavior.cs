using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Behaviour to apply after inserting some predicted text.
    /// </summary>
    public enum PredictionSpacingBehavior
    {
        /// <summary>
        /// Add a following space if the next character typed is not punctuation.
        /// </summary>
        [Description("Add if needed")]
        AddIfNeeded,

        /// <summary>
        /// Add a following space immediately, but remove it if the next character typed is punctuation.
        /// </summary>
        [Description("Add then remove if unneeded")]
        AddAndRemoveIfUnwanted,

        /// <summary>
        /// Add a following space immediately and apply no further logic.
        /// </summary>
        [Description("Always add")]
        Always,

        /// <summary>
        /// Never add a following space.
        /// </summary>
        [Description("Never add")]
        Never
    }
}
