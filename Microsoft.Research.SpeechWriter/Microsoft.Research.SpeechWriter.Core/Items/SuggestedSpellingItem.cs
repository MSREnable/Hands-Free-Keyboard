namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A symbol in the suggestion list.
    /// </summary>
    public class SuggestedSpellingItem : Command<SpellingVocabularySource>
    {
        internal SuggestedSpellingItem(SpellingVocabularySource source, string prefix, int token)
            : base(source)
        {
            Prefix = prefix;
            Symbol = (char)token;
        }

        /// <summary>
        /// The word prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The symbol to be added.
        /// </summary>
        public char Symbol { get; }

        internal override void Execute(SpellingVocabularySource source)
        {
            source.AddSpellingToken(Symbol);
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Prefix + Symbol;
    }
}
