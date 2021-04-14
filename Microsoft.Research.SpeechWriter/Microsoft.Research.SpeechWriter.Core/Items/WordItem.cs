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
            Word = word;

            var upper = word.ToUpper(Culture);
            var lower = word.ToLower(Culture);
            IsCased = Culture.CompareInfo.Compare(upper, lower) != 0;
        }

        /// <summary>
        /// Is this item changed by conversion to uppercase or lowercase?
        /// </summary>
        public override bool IsCased { get; }

        /// <summary>
        /// The text of the word.
        /// </summary>
        public string Word { get; }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => Word;

    }
}