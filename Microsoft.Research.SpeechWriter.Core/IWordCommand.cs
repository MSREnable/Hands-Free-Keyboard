using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Token command carrying a word.
    /// </summary>
    public interface IWordCommand : ICommand
    {
        /// <summary>
        /// The text of an underlying word.
        /// </summary>
        string Word { get; }
    }
}
