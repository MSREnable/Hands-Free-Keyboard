using Microsoft.Research.SpeechWriter.Core.Data;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Unicode item.
    /// </summary>
    public class InterstitialUnicodeItem : Command<UnicodeVocabularySource>
    {
        internal InterstitialUnicodeItem(ITile predecessor, UnicodeVocabularySource source)
            : base(predecessor, source)
        {
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => ":";

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, "\xE775");

        internal override void Execute(UnicodeVocabularySource source)
        {
            source.SetSuggestionsView();
        }
    }
}
