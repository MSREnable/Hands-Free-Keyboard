namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Start item.
    /// </summary>
    public class HeadStartItem : Command<WordVocabularySource>
    {
        internal HeadStartItem(WordVocabularySource source)
            : base(null, source)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.TransferSuccessorsToRunOn(this);
        }
    }
}
