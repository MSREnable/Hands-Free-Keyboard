namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Selected word item.
    /// </summary>
    public class GhostWordItem : WordItem
    {
        internal GhostWordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source, word)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.TakeGhostWord(this);
        }
    }
}