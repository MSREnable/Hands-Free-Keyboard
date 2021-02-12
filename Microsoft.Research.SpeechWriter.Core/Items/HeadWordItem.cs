namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// Selected word item.
    /// </summary>
    public sealed class HeadWordItem : WordItem
    {
        internal HeadWordItem(WordVocabularySource source, IWordCommand word)
            : base(source, word.Word)
        {
        }

        internal HeadWordItem(WordVocabularySource source, string word)
            : base(source, word)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.TransferSuccessorsToRunOn(this);
        }
    }
}