namespace Microsoft.Research.SpeechWriter.Core.Data
{
    /// <summary>
    /// The type of the tile.
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// A normal tile that by default is surrounded by spaces.
        /// </summary>
        Normal,

        /// <summary>
        /// A prefix tile that attaches to the next tile without a space.
        /// </summary>
        Prefix,

        /// <summary>
        /// A suffix tile that attaches to the previous tile without a space.
        /// </summary>
        Suffix,

        /// <summary>
        /// An infix tile that attaches to both the previous and next tile without a space.
        /// </summary>
        Infix,

        /// <summary>
        /// A suggestion suffix tile that will attach itself to the previous word if selected.
        /// </summary>
        Extension,

        /// <summary>
        /// A special command tile.
        /// </summary>
        Command
    }
}
