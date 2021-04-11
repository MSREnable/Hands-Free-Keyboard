namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Word spelling item.
    /// </summary>
    public class InterstitialSpellingItem : Command<OuterSpellingVocabularySource>
    {
        private readonly int _index;

        internal InterstitialSpellingItem(ITile predecessor, OuterSpellingVocabularySource source, int index)
            : base(predecessor, source)
        {
            _index = index;
        }

        internal override void Execute(OuterSpellingVocabularySource source)
        {
            source.StartSpelling(_index);
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "*";
    }
}