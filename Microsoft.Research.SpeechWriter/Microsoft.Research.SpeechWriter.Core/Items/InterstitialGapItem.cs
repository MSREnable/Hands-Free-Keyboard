using System.Diagnostics;

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

        internal InterstitialGapItem(ApplicationModel model, VocabularySource source, int lowerBound, int upperLimit)
            : base(source)
        {
            Debug.Assert(0 <= lowerBound);
            Debug.Assert(lowerBound < upperLimit);

            _model = model;
            _lowerBound = lowerBound;
            _upperLimit = upperLimit;
        }

        internal override void Execute(VocabularySource source)
        {
            _model.SetSuggestionsView(source, _lowerBound, _upperLimit, false);
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ":";
    }
}