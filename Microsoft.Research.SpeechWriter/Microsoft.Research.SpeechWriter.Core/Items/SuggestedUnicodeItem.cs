using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Unicode item.
    /// </summary>
    public class SuggestedUnicodeItem : Command<SpellingVocabularySource>
    {
        internal SuggestedUnicodeItem(ITile predecessor, SpellingVocabularySource source, int code)
            : base(predecessor, source)
        {
            Prefix = source.Prefix;

            Code = code;
        }

        /// <summary>
        /// The spelled prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The numeric value.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// The character value.
        /// </summary>
        public string Symbol => char.ConvertFromUtf32(Code);

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization("TODO");

        internal override void Execute(SpellingVocabularySource source)
        {
            source.AddSymbol(Symbol);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Prefix), Prefix);
            writer.WriteAttributeString(nameof(Code), Code.ToString());
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => Prefix + Symbol;
    }
}
