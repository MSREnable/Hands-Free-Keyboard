using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Suggestion item.
    /// </summary>
    public interface ISuggestionItem : ICommand
    {
        /// <summary>
        /// Create the next suggestion item based on a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The new suggestion item.</returns>
        ISuggestionItem GetNextItem(int token);
    }
}