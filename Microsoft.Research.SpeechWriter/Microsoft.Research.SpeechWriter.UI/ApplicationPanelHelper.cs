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

        private HeadTileLayoutHelper<TControl, TSize, TRect> _head;
        private TailTileLayoutHelper<TControl, TSize, TRect> _tail;
        private InterstitialTileLayoutHelper<TControl, TSize, TRect> _interstitial;
        private SuggestionsLayoutHelper<TControl, TSize, TRect> _suggestions;

        public ApplicationPanelHelper(IApplicationPanel<TControl, TSize, TRect> panel)
        {
            _panel = panel;
        }

        public double Pitch { get; set; } = 110;

        internal double HeadLeft { get; private set; }
        internal double HeadTop { get; private set; }
        internal double HeadRight => InterstitialLeft;
        internal double TailLeft { get; private set; }
        internal double TailTop { get; private set; }
        internal double TailRight => InterstitialLeft;
        internal double InterstitialLeft { get; private set; }
        internal double InterstitialTop { get; private set; }
        internal double InterstitialRight => SuggestionsLeft;

        internal double SuggestionsLeft { get; private set; }
        internal double SuggestionsTop { get; private set; }

        public void SetModel(ApplicationModel value)
        {
            if (_model != null)
            {
                _panel.ResetControls();
            }

            _model = value;

            if (_model != null)
            {
                _head = new HeadTileLayoutHelper<TControl, TSize, TRect>(this, _model.HeadItems);
                _tail = new TailTileLayoutHelper<TControl, TSize, TRect>(this, _model.TailItems);
                _interstitial = new InterstitialTileLayoutHelper<TControl, TSize, TRect>(this, _model.SuggestionInterstitials);
                _suggestions= new SuggestionsLayoutHelper<TControl, TSize, TRect>(this, _model.SuggestionLists);
            }
        }

        public TSize MeasureOverride(TSize availableSize)
        {
            var totalHeight = _panel.HeightFromTSize(availableSize);
            var totalWidth = _panel.WidthFromTSize(availableSize);

            var rows = (int)Math.Floor(_panel.HeightFromTSize(availableSize) / Pitch);
            var height = rows * Pitch;
            var size = _panel.ToTSize(_panel.WidthFromTSize(availableSize), height);

            HeadLeft = 0;
            HeadTop = 0;
            TailLeft = 0;
            TailTop = (rows - 1) * Pitch;
            InterstitialLeft = (totalWidth - Pitch) / 2;
            InterstitialTop = 0;
            SuggestionsLeft = InterstitialLeft + Pitch;
            SuggestionsTop = Pitch / 2.0;

            foreach (var control in _panel.Children)
            {
                _panel.Measure(control, availableSize);
            }

            Debug.WriteLine($"Available panel size is {_panel.WidthFromTSize(availableSize)},{_panel.HeightFromTSize(availableSize)}");
            return size;
        }

        public TSize ArrangeOverride(TSize finalSize)
        {
            _head.Arrange();
            _tail.Arrange();
            _interstitial.Arrange();
            _suggestions.Arrange();

            Debug.WriteLine($"Final panel size is {_panel.WidthFromTSize(finalSize)},{_panel.HeightFromTSize(finalSize)}");
            return finalSize;
        }
    }
}
