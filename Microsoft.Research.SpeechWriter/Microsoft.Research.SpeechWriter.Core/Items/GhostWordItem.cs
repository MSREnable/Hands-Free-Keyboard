using Microsoft.Research.SpeechWriter.Core.Data;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Selected word item.
    /// </summary>
    public class GhostWordItem : WordItem
    {
        internal GhostWordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source, word)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.TakeGhostWord(this);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            var list = new List<string>();
            var me = this;
            do
            {
                list.Insert(0, me.FormattedContent);
                me = me.Predecessor as GhostWordItem;
            }
            while (me != null);

            for (var i = 0; i < list.Count; i++)
            {
                writer.WriteAttributeString($"W{i}", list[i]);
            }
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, TileVisualizationType.Ghosted,
            new TileVisualizationElement(Tile.Type, FormattedContent, TileColor.Text, TileColor.GhostBackground));
    }
}