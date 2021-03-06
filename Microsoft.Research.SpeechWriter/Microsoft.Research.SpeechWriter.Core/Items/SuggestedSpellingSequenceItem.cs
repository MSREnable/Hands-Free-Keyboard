﻿using Microsoft.Research.SpeechWriter.Core.Data;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A symbol in the suggestion list.
    /// </summary>
    public class SuggestedSpellingSequenceItem : Command<SpellingVocabularySource>, ISuggestionItem
    {
        internal SuggestedSpellingSequenceItem(ITile predecessor, SpellingVocabularySource source, string prefix, string symbol)
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
        public override TileVisualization Visualization => new TileVisualization(this, TileType.Normal, Symbol, TileColor.Text, TileColor.SuggestionPartBackground);

        internal override void Execute()
        {
            Source.SetSpellingPrefix(Prefix + Symbol);
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
