using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Behaviour when re-ordering predictions to offer novel top ranked suggestion.
    /// </summary>
    public enum PredictionNovelty
    {
        /// <summary>
        /// When entering a word, change the top ranked word with every keystroke.
        /// </summary>
        [Description("0")]
        FromScratch,

        /// <summary>
        /// When entering a word, change the top ranked word after the first keystroke.
        /// </summary>
        /// <remarks>
        /// When typing the user may not see the initial top ranked word if it starts with
        /// the letter they type.
        /// </remarks>
        [Description("1")]
        FromFirstLetter,

        /// <summary>
        /// Never modify the predicted top ranked word.
        /// </summary>
        [Description("Never")]
        Never
    }
}
