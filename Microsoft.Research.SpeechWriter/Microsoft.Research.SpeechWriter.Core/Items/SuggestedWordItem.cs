using Microsoft.Research.SpeechWriter.Core.Data;
using System;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A word in the suggestion list.
    /// </summary>
    public class SuggestedWordItem : WordItem, ISuggestionItem
    {
        private readonly SuggestedWordItem _previous;

        internal SuggestedWordItem(WordVocabularySource source, SuggestedWordItem previous, string word)
            : base(previous, source, word)
        {
            _previous = previous;
        }

        internal SuggestedWordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source, word)
        {
            _previous = null;
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(Tile.Type, Tile.ToTokenString(), TileColor.SuggestionBackground);

        internal string[] Words
        {
            get
            {
                string[] value;

                if (_previous != null)
                {
                    value = _previous.Words;
                    var index = value.Length;
                    Array.Resize(ref value, index + 1);
                    value[index] = Content;
                }
                else
                {
                    value = new[] { Content };
                }

                return value;
            }
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.AddSuggestedSequence(Words);
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
            var item = Source.GetNextItem(this, token);
            return item;
        }
    }
}
