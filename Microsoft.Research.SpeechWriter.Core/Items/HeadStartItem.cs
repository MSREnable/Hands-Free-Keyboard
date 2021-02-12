namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// Start item.
    /// </summary>
    public class HeadStartItem : Command<WordVocabularySource>
    {
        internal HeadStartItem(WordVocabularySource source)
            : base(source)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.TransferSuccessorsToRunOn(this);
        }
    }
}
