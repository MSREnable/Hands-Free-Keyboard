using Microsoft.Research.SpeechWriter.Core.Data;
using System.Diagnostics;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Gap between word items.
    /// </summary>
    public class InterstitialGapItem : Command<VocabularySource>
    {
        private readonly ApplicationModel _model;
        private readonly int _lowerBound;
        private readonly int _upperLimit;

        internal InterstitialGapItem(ITile predecessor, ApplicationModel model, VocabularySource source, int lowerBound, int upperLimit)
            : base(predecessor, source)
        {
            Debug.Assert(model == source.Model);
            Debug.Assert(0 <= lowerBound);
            Debug.Assert(lowerBound < upperLimit);

            _model = model;
            _lowerBound = lowerBound;
            _upperLimit = upperLimit;
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, ":");

        internal override void Execute(VocabularySource source)
        {
            _model.SetSuggestionsView(source, _lowerBound, _upperLimit, false);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(_lowerBound), _lowerBound.ToString());
            writer.WriteAttributeString(nameof(_upperLimit), _upperLimit.ToString());
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => ":";
    }
}