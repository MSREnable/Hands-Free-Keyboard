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

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, TileVisualizationType.Ghosted, "\xBB");
    }
}
