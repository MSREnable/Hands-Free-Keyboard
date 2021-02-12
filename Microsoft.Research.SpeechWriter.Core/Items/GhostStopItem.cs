namespace Microsoft.Research.RankWriter.Library.Items
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
