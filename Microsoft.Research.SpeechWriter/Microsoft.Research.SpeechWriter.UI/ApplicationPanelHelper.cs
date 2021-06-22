using Microsoft.Research.SpeechWriter.Core;
using System.Diagnostics;
using System.Drawing;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class ApplicationPanelHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal readonly IApplicationPanel<TControl, TSize, TRect> _panel;

        private ApplicationModel _model;

        private InterstitialLayoutHelper<TControl, TSize, TRect> _interstitial;

        public ApplicationPanelHelper(IApplicationPanel<TControl, TSize, TRect> panel)
        {
            _panel = panel;
        }

        public void SetModel(ApplicationModel value)
        {
            if (_model != null)
            {
                _panel.ResetControls();
            }

            _model = value;

            if (_model != null)
            {
                _interstitial = new InterstitialLayoutHelper<TControl, TSize, TRect>(this, _model.SuggestionInterstitials);
            }
        }

        public TSize MeasureOverride(TSize availableSize)
        {
            Debug.WriteLine($"Available panel size is {_panel.WidthFromTSize(availableSize)},{_panel.HeightFromTSize(availableSize)}");
            return availableSize;
        }

        public TSize ArrangeOverride(TSize finalSize)
        {
            Debug.WriteLine($"Final panel size is {_panel.WidthFromTSize(finalSize)},{_panel.HeightFromTSize(finalSize)}");
            return finalSize;
        }
    }
}
