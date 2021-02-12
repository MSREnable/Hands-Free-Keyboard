namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Unicode item.
    /// </summary>
    public class SuggestedUnicodeItem : Command<OuterSpellingVocabularySource>
    {
        internal SuggestedUnicodeItem(OuterSpellingVocabularySource source, int code)
            : base(source)
        {
            Prefix = source.Prefix;

            Symbol = char.ConvertFromUtf32(code);
        }

        /// <summary>
        /// The spelled prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The character value.
        /// </summary>
        public string Symbol { get; }

        internal override void Execute(OuterSpellingVocabularySource source)
        {
            source.AddSymbol(Symbol);
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Prefix + Symbol;
    }
}
