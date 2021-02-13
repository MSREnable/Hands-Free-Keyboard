﻿namespace Microsoft.Research.SpeechWriter.Core.Items
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

            Code = code;
        }

        /// <summary>
        /// The spelled prefix.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// The numeric value.
        /// </summary>
        public int Code { get; }

        /// <summary>
        /// The character value.
        /// </summary>
        public string Symbol => char.ConvertFromUtf32(Code);

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
