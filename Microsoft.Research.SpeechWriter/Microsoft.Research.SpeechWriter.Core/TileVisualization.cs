using Microsoft.Research.SpeechWriter.Core.Data;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Description of how tile is to be visualized.
    /// </summary>
    public class TileVisualization
    {
        internal TileVisualization(TileVisualizationType type, params TileVisualizationElement[] elements)
        {
            Debug.Assert(elements != null);
            Debug.Assert((elements.Length != 0) == (type != TileVisualizationType.Hidden));

            Type = type;
            Elements = elements;
        }

        internal TileVisualization(TileType type, string text, TileColor foreground, TileColor background)
            : this(new TileVisualizationElement(type, text, foreground, background))
        {
        }

        internal TileVisualization(params TileVisualizationElement[] elements)
            : this(TileVisualizationType.Normal, elements)
        {
        }

        internal TileVisualization(TileType type, string text, TileColor background)
            : this(new TileVisualizationElement(type, text, TileColor.Text, background))
        {
        }

        internal TileVisualization(string text)
            : this(new TileVisualizationElement(text))
        {
        }

        internal TileVisualization(TileVisualizationType type, string text)
            : this(type, new TileVisualizationElement(text))
        {
        }

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