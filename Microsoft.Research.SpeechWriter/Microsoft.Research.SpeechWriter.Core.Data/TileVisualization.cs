using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    /// <summary>
    /// Description of how tile is to be visualized.
    /// </summary>
    public class TileVisualization
    {
        public TileVisualization(ICommand command, TileVisualizationType type, params TileVisualizationElement[] elements)
        {
            Debug.Assert(elements != null);
            Debug.Assert((elements.Length != 0) == (type != TileVisualizationType.Hidden));

            Command = command;
            Type = type;
            Elements = elements;
        }

        public TileVisualization(ICommand command, TileType type, string text, TileColor foreground, TileColor background)
            : this(command, new TileVisualizationElement(type, text, foreground, background))
        {
        }

        public TileVisualization(ICommand command, params TileVisualizationElement[] elements)
            : this(command, TileVisualizationType.Normal, elements)
        {
        }

        public TileVisualization(ICommand command, TileType type, string text, TileColor background)
            : this(command, new TileVisualizationElement(type, text, TileColor.Text, background))
        {
        }

        public TileVisualization(ICommand command, string text)
            : this(command, new TileVisualizationElement(text))
        {
        }

        public TileVisualization(ICommand command, TileVisualizationType type, string text)
            : this(command, type, new TileVisualizationElement(text))
        {
        }

        /// <summary>
        /// The associated command.
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// Is tile to be rendered as disabled to indicate it is ghosted, hidden or as a normal tile.
        /// </summary>
        public TileVisualizationType Type { get; }

        /// <summary>
        /// Get the one or more elements that constitute the tile.
        /// </summary>
        public IEnumerable<TileVisualizationElement> Elements { get; }
    }
}