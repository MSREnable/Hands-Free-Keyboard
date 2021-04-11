namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A symbol in the suggestion list.
    /// </summary>
    public class SuggestedSpellingSequenceItem : Command<SpellingVocabularySource>, ISuggestionItem
    {
        internal SuggestedSpellingSequenceItem(ITile predecessor, SpellingVocabularySource source, string prefix, string symbol) 
            : base(predecessor,  source)
        {
            Prefix = prefix;
            Symbol = symbol;
        }

        /// <summary>
        /// The word prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The symbol to be added.
        /// </summary>
        public string Symbol { get; }

        internal override void Execute(SpellingVocabularySource source)
        {
            source.SetSpellingPrefix(Prefix + Symbol);
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Prefix + Symbol;

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            var item = Source.GetNextItem(this, Prefix + Symbol, token);
            return item;
        }
    }
}
