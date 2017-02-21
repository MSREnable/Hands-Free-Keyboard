namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Abstraction of the system clipboard.
    /// </summary>
    public interface IClipboardProvider
    {

        /// <summary>
        /// Get a string from the clipboard.
        /// </summary>
        /// <returns>String from clipboard or null if no string available.</returns>
        string GetText();

        /// <summary>
        /// Set a string to the clipboard.
        /// </summary>
        /// <param name="text">The string to set.</param>
        void SetText(string text);
    }
}
