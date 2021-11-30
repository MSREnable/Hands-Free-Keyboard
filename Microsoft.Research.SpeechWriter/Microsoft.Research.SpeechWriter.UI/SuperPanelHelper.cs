using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class SuperPanelHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        private readonly ISuperPanel<TControl, TSize, TRect> _panel;

        private ApplicationModel _model;

        private HeadTileLayoutHelper<TControl, TSize, TRect> _headPanelHelper;
        private TailTileLayoutHelper<TControl, TSize, TRect> _tailPanelHelper;
        private InterstitialTileLayoutHelper<TControl, TSize, TRect> _interstitialPanelHelper;
        private SuggestionsLayoutHelper<TControl, TSize, TRect> _suggestionsPanelHelper;

        private int _rows;
        private int _columns;

        public SuperPanelHelper(ISuperPanel<TControl, TSize, TRect> panel)
        {
            _panel = panel;
        }

        internal double HorizontalPitch => _panel.HorizontalPitch;

        internal double VerticalPitch => _panel.VerticalPitch;

        internal int SelectedHeadIndex => _model.SelectedHeadIndex;

        public void SetModel(ApplicationModel model)
        {
            _model = model;

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
            var availableWidth = _panel.GetWidth(availableSize);
            var availableHeight = _panel.GetHeight(availableSize);

            var horizontalPitch = _panel.HorizontalPitch;
            var verticalPitch = _panel.VerticalPitch;

            var rows = Math.Floor(availableHeight / verticalPitch);
            var finalHeight = rows * verticalPitch;
            var sideWidth = (availableWidth - horizontalPitch) / 2.0;

            _rows = (int)rows;
            _columns = (int)Math.Floor(availableWidth / horizontalPitch);

            _interstitialPanelHelper._panel.Measure(_panel.CreateSize(horizontalPitch, finalHeight));
            _headPanelHelper._panel.Measure(_panel.CreateSize(sideWidth, finalHeight - verticalPitch));
            _tailPanelHelper._panel.Measure(_panel.CreateSize(sideWidth, verticalPitch));
            _suggestionsPanelHelper._panel.Measure(_panel.CreateSize(sideWidth, finalHeight - verticalPitch));

            return _interstitialPanelHelper._panel.CreateSize(availableWidth, finalHeight);
        }

        public TSize ArrangeOverride(TSize finalSize)
        {
            Debug.WriteLine($"SuperPanel.ArrangeOverride {finalSize}");

            var horizontalPitch = _panel.HorizontalPitch;
            var verticalPitch = _panel.VerticalPitch;

            var finalHeight = _panel.GetHeight(_interstitialPanelHelper._panel.DesiredSize);
            var finalWidth = _panel.GetWidth(finalSize);
            var sideWidth = (finalWidth - horizontalPitch) / 2;

            _headPanelHelper._panel.Arrange(_panel.CreateRect(0, 0, sideWidth, finalHeight - verticalPitch));
            _tailPanelHelper._panel.Arrange(_panel.CreateRect(0, finalHeight - verticalPitch, sideWidth, verticalPitch));
            _interstitialPanelHelper._panel.Arrange(_panel.CreateRect(sideWidth, 0, horizontalPitch, finalHeight));
            _suggestionsPanelHelper._panel.Arrange(_panel.CreateRect(sideWidth + horizontalPitch, verticalPitch / 2.0, sideWidth, finalHeight - verticalPitch));

            _model.DisplayRows = _rows;
            _model.DisplayColumns = _columns;

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

            var rect = _panel.GetControlRect(target, control);

            return rect;
        }
    }
}
