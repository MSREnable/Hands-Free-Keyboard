using Microsoft.Research.SpeechWriter.Core.Data;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Backspace spelling error.
    /// </summary>
    public class SuggestedSpellingBackspaceItem : Command<SpellingVocabularySource>, ISuggestionItem
    {
        internal SuggestedSpellingBackspaceItem(ITile predecessor, SpellingVocabularySource source, string prefix)
            : base(predecessor, source)
        {
            Prefix = prefix.Substring(0, prefix.Length - 1);
        }

        /// <summary>
        /// The word prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, TileType.Command, Prefix, TileColor.GrayText, TileColor.None);

        internal override void Execute()
        {
            Source.SpellingBackspace();
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Prefix), Prefix);
        }

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            return null;
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => Prefix;
    }
}