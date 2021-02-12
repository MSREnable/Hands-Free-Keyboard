using System.Diagnostics;

namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// Complete word.
    /// </summary>
    public class SuggestedSpellingWordItem : Command<WordVocabularySource>
    {
        internal SuggestedSpellingWordItem(WordVocabularySource source, string prefix)
            : base(source)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(prefix));

            Word = prefix;
        }

        /// <summary>
        /// The word to be committed.
        /// </summary>
        public string Word { get; }

        internal override void Execute(WordVocabularySource source)
        {
            source.AddSuggestedWord(Word);
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Word;
    }
}