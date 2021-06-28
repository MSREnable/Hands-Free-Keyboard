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

        private readonly IApplicationPanel<TControl, TSize, TRect> _headPanel;
        private readonly IApplicationPanel<TControl, TSize, TRect> _tailPanel;
        private readonly IApplicationPanel<TControl, TSize, TRect> _interstitialPanel;
        private readonly IApplicationPanel<TControl, TSize, TRect> _suggestionsPanel;

        public SuperPanelHelper(ISuperPanel<TControl, TSize, TRect> panel)
        {
            _panel = panel;

            _headPanel = panel.CreateChild();
            _tailPanel = panel.CreateChild();
            _interstitialPanel = panel.CreateChild();
            _suggestionsPanel = panel.CreateChild();
        }

        public void SetModel(ApplicationModel model)
        {
            _suggestionsPanel.Model = model;
        }

        public TSize MeasureOverride(TSize availableSize)
        {
            Debug.WriteLine($"SuperPanel.MeasureOverride {availableSize}");

            _interstitialPanel.Measure(availableSize);

            var pitch = _interstitialPanel.GetWidth(_interstitialPanel.DesiredSize);
            pitch = 110;
            var totalHeight = _interstitialPanel.GetHeight(_interstitialPanel.DesiredSize);
            var availableWidth = _interstitialPanel.GetWidth(availableSize);
            var sideWidth = (availableWidth - pitch) / 2;

            _headPanel.Measure(_interstitialPanel.CreateSize(sideWidth, totalHeight - pitch));
            _tailPanel.Measure(_interstitialPanel.CreateSize(sideWidth, pitch));
            _suggestionsPanel.Measure(_interstitialPanel.CreateSize(sideWidth, totalHeight - pitch));

            return _interstitialPanel.CreateSize(availableWidth, totalHeight);
        }

        public TSize ArrangeOverride(TSize finalSize)
        {
            Debug.WriteLine($"SuperPanel.ArrangeOverride {finalSize}");

            var pitch = _interstitialPanel.GetWidth(_interstitialPanel.DesiredSize);
            pitch = 110;
            var totalHeight = _interstitialPanel.GetHeight(_interstitialPanel.DesiredSize);
            var finalWidth = _interstitialPanel.GetWidth(finalSize);
            var sideWidth = (finalWidth - pitch) / 2;

            var top = (_interstitialPanel.GetHeight(finalSize) - totalHeight) / 2;

            _headPanel.Arrange(_interstitialPanel.CreateRect(0, top, sideWidth, totalHeight - pitch));
            _tailPanel.Arrange(_interstitialPanel.CreateRect(0, top + totalHeight - pitch, sideWidth, pitch));
            _interstitialPanel.Arrange(_interstitialPanel.CreateRect(sideWidth, top, pitch, totalHeight));
            _suggestionsPanel.Arrange(_interstitialPanel.CreateRect(sideWidth + pitch, top + pitch / 2, sideWidth, totalHeight - pitch));

            return _interstitialPanel.CreateSize(finalWidth, totalHeight);
        }

        public TRect GetTargetRect(TControl target, ApplicationRobotAction action)
        {
            throw new System.NotImplementedException();
        }
    }
}
