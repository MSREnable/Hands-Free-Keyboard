using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class ApplicationPanelHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal readonly IApplicationPanel<TControl, TSize, TRect> _panel;

        private ApplicationModel _model;

        private InterstitialTileLayoutHelper<TControl, TSize, TRect> _interstitial;

        public ApplicationPanelHelper(IApplicationPanel<TControl, TSize, TRect> panel)
        {
            _panel = panel;
        }

        public double Pitch { get; set; } = 110;

        internal double InterstitialLeft { get; private set; }
        internal double InterstitialTop { get; private set; }

        public void SetModel(ApplicationModel value)
        {
            if (_model != null)
            {
                _panel.ResetControls();
            }

            _model = value;

            if (_model != null)
            {
                _interstitial = new InterstitialTileLayoutHelper<TControl, TSize, TRect>(this, _model.SuggestionInterstitials);
            }
        }

        public TSize MeasureOverride(TSize availableSize)
        {
            var totalHeight = _panel.HeightFromTSize(availableSize);
            var totalWidth = _panel.WidthFromTSize(availableSize);

            var rows = (int)Math.Floor(_panel.HeightFromTSize(availableSize) / Pitch);
            var height = rows * Pitch;
            var size = _panel.ToTSize(_panel.WidthFromTSize(availableSize), height);

            InterstitialLeft = (totalWidth - Pitch) / 2;
            InterstitialTop = 0;

            foreach (var control in _panel.Children)
            {
                _panel.Measure(control, availableSize);
            }

            Debug.WriteLine($"Available panel size is {_panel.WidthFromTSize(availableSize)},{_panel.HeightFromTSize(availableSize)}");
            return size;
        }

        public TSize ArrangeOverride(TSize finalSize)
        {
            _interstitial.Arrange();

            Debug.WriteLine($"Final panel size is {_panel.WidthFromTSize(finalSize)},{_panel.HeightFromTSize(finalSize)}");
            return finalSize;
        }
    }
}
