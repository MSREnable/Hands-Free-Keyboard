namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Start item.
    /// </summary>
    public class GhostStopItem : TailStopItem
    {
        internal GhostStopItem(ITile predecessor, WordVocabularySource source)
            : base(predecessor, source)
        {
        }
    }
}
