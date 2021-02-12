namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// Backspace spelling error.
    /// </summary>
    public class SuggestedSpellingBackspaceItem : Command<SpellingVocabularySource>
    {
        internal SuggestedSpellingBackspaceItem(SpellingVocabularySource source, string prefix)
            : base(source)
        {
            Prefix = prefix.Substring(0, prefix.Length - 1);
        }

        /// <summary>
        /// The word prefix.
        /// </summary>
        public string Prefix { get; }

        internal override void Execute(SpellingVocabularySource source)
        {
            source.SpellingBackspace();
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Prefix;
    }
}