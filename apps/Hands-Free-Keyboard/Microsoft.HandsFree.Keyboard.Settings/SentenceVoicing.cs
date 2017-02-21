using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Different ways a sentence can be voiced.
    /// </summary>
    public enum SentenceBehavior
    {
        /// <summary>
        /// Say the command name.
        /// </summary>
        [Description("Never read")]
        Command,

        /// <summary>
        /// Always speack the sentence text.
        /// </summary>
        [Description("Always read")]
        Always,

        /// <summary>
        /// Only say the sentence when it is repeated.
        /// </summary>
        [Description("Read when repeated")]
        OnlyRepetition
    }
}
