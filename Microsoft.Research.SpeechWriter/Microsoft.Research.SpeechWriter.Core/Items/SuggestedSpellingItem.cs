using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A symbol in the suggestion list.
    /// </summary>
    public class SuggestedSpellingItem : Command<SpellingVocabularySource>, ISuggestionItem
    {
        internal SuggestedSpellingItem(ITile predecessor, SpellingVocabularySource source, string prefix, string symbol)
            : base(predecessor, source)
        {
            Prefix = prefix;
            Symbol = symbol;
        }

        /// <summary>
        /// The word prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The symbol to be added.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization("TODO");

        internal override void Execute(SpellingVocabularySource source)
        {
            source.SetSpellingPrefix(Prefix + Symbol);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Prefix), Prefix);
            writer.WriteAttributeString(nameof(Symbol), Symbol);
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => Prefix + Symbol;

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            var item = Source.GetNextItem(this, Prefix + Symbol, token);
            return item;
        }
    }
}
