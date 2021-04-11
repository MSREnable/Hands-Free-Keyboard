namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Selected word item.
    /// </summary>
    public sealed class HeadWordItem : WordItem
    {
        internal HeadWordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source, word)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.TransferSuccessorsToRunOn(this);
        }
    }
}