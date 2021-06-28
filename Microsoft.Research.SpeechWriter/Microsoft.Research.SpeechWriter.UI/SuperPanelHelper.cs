using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class SuperPanelHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal readonly ISuperPanel<TControl, TSize, TRect> _panel;

        private HeadTileLayoutHelper<TControl, TSize, TRect> _headPanelHelper;
        private TailTileLayoutHelper<TControl, TSize, TRect> _tailPanelHelper;
        private InterstitialTileLayoutHelper<TControl, TSize, TRect> _interstitialPanelHelper;
        private SuggestionsLayoutHelper<TControl, TSize, TRect> _suggestionsPanelHelper;

        public SuperPanelHelper(ISuperPanel<TControl, TSize, TRect> panel)
        {
            _panel = panel;
        }

        public void SetModel(ApplicationModel model)
        {
            if (model != null)
            {
                _panel.ResetChildren();
                _headPanelHelper = new HeadTileLayoutHelper<TControl, TSize, TRect>(this, model.HeadItems);
                _tailPanelHelper = new TailTileLayoutHelper<TControl, TSize, TRect>(this, model.TailItems);
                _interstitialPanelHelper = new InterstitialTileLayoutHelper<TControl, TSize, TRect>(this, model.SuggestionInterstitials);
                _suggestionsPanelHelper = new SuggestionsLayoutHelper<TControl, TSize, TRect>(this, model.SuggestionLists);
            }
        }

        internal IApplicationPanel<TControl, TSize, TRect> CreateChild(ApplicationPanelHelper<TControl, TSize, TRect> helper)
        {
            var child = _panel.CreateChild(helper);
            return child;
        }

        public TSize MeasureOverride(TSize availableSize)
        {
            Debug.WriteLine($"SuperPanel.MeasureOverride {availableSize}");

            _interstitialPanelHelper._panel.Measure(availableSize);

            var pitch = _interstitialPanelHelper._panel.GetWidth(_interstitialPanelHelper._panel.DesiredSize);
            pitch = 110;
            var totalHeight = _interstitialPanelHelper._panel.GetHeight(_interstitialPanelHelper._panel.DesiredSize);
            var availableWidth = _interstitialPanelHelper._panel.GetWidth(availableSize);
            var sideWidth = (availableWidth - pitch) / 2;

            _headPanelHelper._panel.Measure(_interstitialPanelHelper._panel.CreateSize(sideWidth, totalHeight - pitch));
            _tailPanelHelper._panel.Measure(_interstitialPanelHelper._panel.CreateSize(sideWidth, pitch));
            _suggestionsPanelHelper._panel.Measure(_interstitialPanelHelper._panel.CreateSize(sideWidth, totalHeight - pitch));

            return _interstitialPanelHelper._panel.CreateSize(availableWidth, totalHeight);
        }

        public TSize ArrangeOverride(TSize finalSize)
        {
            Debug.WriteLine($"SuperPanel.ArrangeOverride {finalSize}");

            var pitch = _interstitialPanelHelper._panel.GetWidth(_interstitialPanelHelper._panel.DesiredSize);
            pitch = 110;
            var totalHeight = _interstitialPanelHelper._panel.GetHeight(_interstitialPanelHelper._panel.DesiredSize);
            var finalWidth = _interstitialPanelHelper._panel.GetWidth(finalSize);
            var sideWidth = (finalWidth - pitch) / 2;

            var top = (_interstitialPanelHelper._panel.GetHeight(finalSize) - totalHeight) / 2;

            _headPanelHelper._panel.Arrange(_interstitialPanelHelper._panel.CreateRect(0, top, sideWidth, totalHeight - pitch));
            _tailPanelHelper._panel.Arrange(_interstitialPanelHelper._panel.CreateRect(0, top + totalHeight - pitch, sideWidth, pitch));
            _interstitialPanelHelper._panel.Arrange(_interstitialPanelHelper._panel.CreateRect(sideWidth, top, pitch, totalHeight));
            _suggestionsPanelHelper._panel.Arrange(_interstitialPanelHelper._panel.CreateRect(sideWidth + pitch, top + pitch / 2, sideWidth, totalHeight - pitch));

            return finalSize;
        }

        public TRect GetTargetRect(TControl target, ApplicationRobotAction action)
        {
            TControl control;

            switch (action.Target)
            {
                case ApplicationRobotActionTarget.Head:
                    control = _headPanelHelper.GetControl(action.Index);
                    break;

                case ApplicationRobotActionTarget.Tail:
                    control = _tailPanelHelper.GetControl(action.Index);
                    break;

                case ApplicationRobotActionTarget.Interstitial:
                    control = _interstitialPanelHelper.GetControl(action.Index);
                    break;

                case ApplicationRobotActionTarget.Suggestion:
                default:
                    Debug.Assert(action.Target == ApplicationRobotActionTarget.Suggestion);
                    control = _suggestionsPanelHelper.GetControl(action.Index, action.SubIndex);
                    break;
            }

            var rect = _headPanelHelper._panel.CreateRect(target, control);

            return rect;
        }
    }
}
