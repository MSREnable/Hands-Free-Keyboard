namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// Selected word item.
    /// </summary>
    public class GhostWordItem : WordItem
    {
        internal GhostWordItem(WordVocabularySource source, string word)
            : base(source, word)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.TransferRunOnToSuccessors(this);
        }
    }
}