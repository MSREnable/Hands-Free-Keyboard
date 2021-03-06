﻿using Microsoft.Research.SpeechWriter.Core.Data;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A word in the suggestion list for spelling where sorting semantics are slightly different..
    /// </summary>
    public class SuggestedSpellingWordItem : WordItem, ISuggestionItem
    {
        internal SuggestedSpellingWordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source, word)
        {
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, TileType.Normal, FormattedContent, TileColor.SuggestionBackground);

        internal override void Execute()
        {
            Source.AddSuggestedWord(Content);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(Content), Content);
        }

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            return null;
        }
    }
}
