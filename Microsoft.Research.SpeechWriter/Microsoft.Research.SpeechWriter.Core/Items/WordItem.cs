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

            Tile = TileData.FromTokenString(word);

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
        /// The underlying tile.
        /// </summary>
        public TileData Tile { get; }

        /// <summary>
        /// Is this item changed by conversion to uppercase or lowercase?
        /// </summary>
        public override bool IsCased { get; }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => Tile.ToTokenString();

        /// <summary>
        /// The unformatted content part.
        /// </summary>
        public string UnformattedContent => Tile.Content;

        /// <summary>
        /// The formatted content of the tile.
        /// </summary>
        public override string FormattedContent => IsCased && !IsCasedSuccessor ? Culture.TextInfo.ToTitleCase(UnformattedContent) : UnformattedContent;
    }
}