namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A symbol in the suggestion list.
    /// </summary>
    public class SuggestedSpellingSequenceItem : Command<SpellingVocabularySource>
    {
        readonly int[] _tokens;

        internal SuggestedSpellingSequenceItem(SpellingVocabularySource source, int[] tokens)
            : base(source)
        {
            _tokens = tokens;
        }

        /// <summary>
        /// The word prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The symbol to be added.
        /// </summary>
        public string Symbol => ((char)_tokens[_tokens.Length - 1]).ToString();

        internal override void Execute(SpellingVocabularySource source)
        {
            source.AddSpellingTokens(_tokens);
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Prefix + Symbol;
    }
}
