using Microsoft.Research.SpeechWriter.Core.Data;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Whole word item.
    /// </summary>
    public abstract class WordItem : Command<WordVocabularySource>
    {
        internal WordItem(ITile predecessor, WordVocabularySource source, string tokenString)
            : base(predecessor, source)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(tokenString));

            var tile = TileData.FromTokenString(tokenString);
            Content = tile.Content;
            IsAttachedToNext = tile.IsGlueAfter;
            IsAttachedToPrevoius = tile.IsGlueBefore;

            var word = tile.Content;
            var length = word.Length;
            var index = 0;
            while (index < length && !char.IsLetterOrDigit(word, index))
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
        public bool IsAttachedToPrevoius { get; }

        /// <summary>
        /// The formatted content of the tile.
        /// </summary>
        public override string FormattedContent => IsCased && !IsCasedSuccessor ? Culture.TextInfo.ToTitleCase(Content) : Content;
    }
}