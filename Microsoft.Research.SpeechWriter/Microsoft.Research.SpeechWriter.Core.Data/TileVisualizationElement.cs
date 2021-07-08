namespace Microsoft.Research.SpeechWriter.Core.Data
{
    /// <summary>
    /// Labelling component of tile visualization.
    /// </summary>
    public class TileVisualizationElement
    {
        public TileVisualizationElement(TileType type,
            string text,
            TileColor foreground,
            TileColor background)
        {
            Type = type;
            Text = text;
            Foreground = foreground;
            Background = background;
        }

        public TileVisualizationElement(string text,
            TileColor foreground)
            : this(TileType.Command, text, foreground, TileColor.None)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public TileVisualizationElement(string text)
            : this(TileType.Command, text, TileColor.Text, TileColor.None)
        {
        }

        /// <summary>
        /// The visual element type.
        /// </summary>
        public TileType Type { get; }

        /// <summary>
        /// The text of the tile.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The logical foreground (text) color of the element.
        /// </summary>
        public TileColor Foreground { get; }

        /// <summary>
        /// The logical background color of the element.
        /// </summary>
        public TileColor Background { get; }
    }
}