using Microsoft.Research.SpeechWriter.Core.Data;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Word spelling item.
    /// </summary>
    public class InterstitialSpellingItem : Command<SpellingVocabularySource>
    {
        private readonly int _index;

        internal InterstitialSpellingItem(ITile predecessor, SpellingVocabularySource source, int index)
            : base(predecessor, source)
        {
            _index = index;
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, "*");

        internal override void Execute()
        {
            Source.StartSpelling(_index);
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => "*";
    }
}