using Microsoft.Research.SpeechWriter.Core.Data;
using System;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A word in the suggestion list.
    /// </summary>
    public class ExtendedSuggestedWordItem : SuggestedWordItem, ISuggestionItem
    {
        private readonly string _partWord;

        internal ExtendedSuggestedWordItem(WordVocabularySource source, SuggestedWordItem previous, string wholeWord, string partWord)
            : base(source, previous, wholeWord)
        {
            _partWord = partWord;
        }

        internal ExtendedSuggestedWordItem(ITile predecessor, WordVocabularySource source, string wholeWord, string partWord)
            : base(predecessor, source, wholeWord)
        {
            _partWord = partWord;
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, TileType.Suffix, _partWord, TileColor.SuggestionBackground);

        internal override void Execute()
        {
            Source.AddSuggestedSequence(Words);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            var words = Words;

            for (var i = 0; i < words.Length; i++)
            {
                writer.WriteAttributeString($"S{i}", words[i].AttributeEscape());
            }
        }

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            throw new NotImplementedException();
        }
    }
}
