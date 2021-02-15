namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Start item.
    /// </summary>
    public class GhostStopItem : TailStopItem
    {
        internal GhostStopItem(WordVocabularySource source, params string[] words)
            : base(source, words)
        {
        }
    }
}
