namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// Word spelling item.
    /// </summary>
    public class InterstitialSpellingItem : Command<OuterSpellingVocabularySource>
    {
        private readonly int _index;

        internal InterstitialSpellingItem(OuterSpellingVocabularySource source, int index)
            : base(source)
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