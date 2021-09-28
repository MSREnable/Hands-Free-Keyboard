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

        internal override void Execute()
        {
            Source.SetSuggestionsView(_lowerBound, _upperLimit, false);
        }

        private void TraceChild(XmlWriter writer, int index)
        {
            var item = (Command)Source.GetIndexItemForTrace(index);
            if (item != null)
            {
                var type = item.GetType();
                var name = type.Name;
                writer.WriteStartElement(name);
                item.TraceContent(writer);
                writer.WriteEndElement();
            }
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(_lowerBound), _lowerBound.ToString());
            writer.WriteAttributeString(nameof(_upperLimit), _upperLimit.ToString());

            TraceChild(writer, _lowerBound);
            TraceChild(writer, _upperLimit - 1);
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => ":";
    }
}