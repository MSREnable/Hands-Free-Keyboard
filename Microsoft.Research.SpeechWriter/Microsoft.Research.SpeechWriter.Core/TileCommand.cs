namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Commands for word tiles.
    /// </summary>
    public enum TileCommand
    {
        /// <summary>
        /// Alter the case or spacing of the current word.
        /// </summary>
        Typing,

        /// <summary>
        /// Numeric code insertion.
        /// </summary>
        Code,

        /// <summary>
        /// Alter behaviour of application
        /// </summary>
        Settings
    }
}
