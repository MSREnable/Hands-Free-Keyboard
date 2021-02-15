namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Whole word item.
    /// </summary>
    public abstract class WordItem : Command<WordVocabularySource>, IWordCommand
    {
        internal WordItem(WordVocabularySource source, string word)
            : base(source)
        {
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