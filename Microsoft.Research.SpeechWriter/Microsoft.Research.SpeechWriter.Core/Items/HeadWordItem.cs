using Microsoft.Research.SpeechWriter.Core.Data;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Selected word item.
    /// </summary>
    public sealed class HeadWordItem : WordItem
    {
        internal HeadWordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source, word)
        {
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, Tile.Type, FormattedContent, TileColor.HeadBackground);

        internal override void Execute(WordVocabularySource source)
        {
            source.TransferSuccessorsToRunOn(this);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            var list = new List<string>();
            var me = this;
            do
            {
                list.Insert(0, me.FormattedContent);
                me = me.Predecessor as HeadWordItem;
            }
            while (me != null);

            for (var i = 0; i < list.Count; i++)
            {
                writer.WriteAttributeString($"W{i}", list[i]);
            }
        }
    }
}