using Microsoft.Research.SpeechWriter.Core.Data;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Tile rewriting item.
    /// </summary>
    public class ReplacementItem : Command<WordVocabularySource>
    {
        private readonly string _replacement;

        internal ReplacementItem(HeadWordItem predecessor, WordVocabularySource source, string replacement)
            : base(predecessor, source)
        {
            _replacement = replacement;

            Tile = TileData.FromTokenString(replacement);
            UnformattedContent = Tile.Content;
            IsAttachedToNext = Tile.IsPrefix;
            IsAttachedToPrevious = Tile.IsSuffix;

            var oldTile = TileData.FromTokenString(predecessor.Content);
            OldUnformattedContent = oldTile.Content;
            OldIsAttachedToNext = oldTile.IsPrefix;
            OldIsAttachedToPrevious = oldTile.IsSuffix;
        }

        /// <summary>
        /// The content.
        /// </summary>
        public override string Content => _replacement;

        /// <summary>
        /// The proposed text.
        /// </summary>
        public string UnformattedContent { get; }

        /// <summary>
        /// The proposed forward attachement.
        /// </summary>
        public bool IsAttachedToNext { get; }

        /// <summary>
        /// The proposed previous attachment.
        /// </summary>
        public bool IsAttachedToPrevious { get; }

        /// <summary>
        /// The original text.
        /// </summary>
        public string OldUnformattedContent { get; }

        /// <summary>
        /// The original forward attachement.
        /// </summary>
        public bool OldIsAttachedToNext { get; }

        /// <summary>
        /// The original previous attachment.
        /// </summary>
        public bool OldIsAttachedToPrevious { get; }

        private TileData Tile { get; }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization =>
            new TileVisualization(new TileVisualizationElement(((HeadWordItem)Predecessor).Tile.Type,
                OldUnformattedContent, TileColor.Text, TileColor.HeadBackground),
            new TileVisualizationElement("\x2192"),
            new TileVisualizationElement(Tile.Type, Tile.Content, TileColor.Text, TileColor.HeadBackground));

        internal override void Execute(WordVocabularySource source)
        {
            source.ReplaceLastItem(_replacement);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(OldUnformattedContent), OldUnformattedContent);
            writer.WriteAttributeString(nameof(UnformattedContent), UnformattedContent);
        }
    }
}
