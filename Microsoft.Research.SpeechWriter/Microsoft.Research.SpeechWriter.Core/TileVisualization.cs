using Microsoft.Research.SpeechWriter.Core.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Description of how tile is to be visualized.
    /// </summary>
    public class TileVisualization
    {
        internal TileVisualization(ICommand command, TileVisualizationType type, params TileVisualizationElement[] elements)
        {
            Debug.Assert(elements != null);
            Debug.Assert((elements.Length != 0) == (type != TileVisualizationType.Hidden));

            Command = command;
            Type = type;
            Elements = elements;
        }

        internal TileVisualization(ICommand command, TileType type, string text, TileColor foreground, TileColor background)
            : this(command, new TileVisualizationElement(type, text, foreground, background))
        {
        }

        internal TileVisualization(ICommand command, params TileVisualizationElement[] elements)
            : this(command, TileVisualizationType.Normal, elements)
        {
        }

        internal TileVisualization(ICommand command, TileType type, string text, TileColor background)
            : this(command, new TileVisualizationElement(type, text, TileColor.Text, background))
        {
        }

        internal TileVisualization(ICommand command, string text)
            : this(command, new TileVisualizationElement(text))
        {
        }

        internal TileVisualization(ICommand command, TileVisualizationType type, string text)
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