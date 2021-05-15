using Microsoft.Research.SpeechWriter.Core.Data;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Whole word item.
    /// </summary>
    public abstract class WordItem : Command<WordVocabularySource>
    {
        internal WordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(word));

            var tile = TileData.FromTokenString(word);
            Content = word;
            UnformattedContent = tile.Content;
            IsAttachedToNext = tile.IsGlueAfter;
            IsAttachedToPrevious = tile.IsGlueBefore;

            var text = UnformattedContent;
            var length = text.Length;
            var index = 0;
            while (index < length && !char.IsLetterOrDigit(text, index))
            {
                index++;
            }
            IsCased = index < length;
        }

        /// <summary>
        /// Is this item changed by conversion to uppercase or lowercase?
        /// </summary>
        public override bool IsCased { get; }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content { get; }

        /// <summary>
        /// Is this item followed with out space by the next item.
        /// </summary>
        public bool IsAttachedToNext { get; }

        /// <summary>
        /// Does this item follow without space the preceeding item.
        /// </summary>
        public bool IsAttachedToPrevious { get; }

        /// <summary>
        /// The unformatted content part.
        /// </summary>
        public string UnformattedContent { get; }

        /// <summary>
        /// The formatted content of the tile.
        /// </summary>
        public override string FormattedContent => IsCased && !IsCasedSuccessor ? Culture.TextInfo.ToTitleCase(UnformattedContent) : UnformattedContent;
    }
}