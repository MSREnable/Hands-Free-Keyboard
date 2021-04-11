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
        }

        /// <summary>
        /// The text of the word.
        /// </summary>
        public string Word { get; }

        /// <summary>
        /// String.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Word;
    }
}