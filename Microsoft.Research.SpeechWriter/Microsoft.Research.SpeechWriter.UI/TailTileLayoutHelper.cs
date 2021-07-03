using Microsoft.Research.SpeechWriter.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    internal class TailTileLayoutHelper<TControl, TSize, TRect> : TileLayoutHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal TailTileLayoutHelper(SuperPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<ITile> list)
            : base(panel, list)
        {
        }

        public override TSize ArrangeOverride(TSize finalSize)
        {
            var reversedControls = new List<TControl>(_controls);
            reversedControls.Reverse();

            var panelWidth = _panel.GetWidth(finalSize);
            var x = panelWidth;
            var y = 0.0;

            foreach (var control in reversedControls)
            {
                var controlSize = _panel.GetDesiredSize(control);
                var controlWidth = _panel.GetWidth(controlSize);
                var controlHeight = _panel.GetHeight(controlSize);

                var left = x - controlWidth;
                if (left < 0 && x != panelWidth)
                {
                    y -= controlHeight;
                    left = x - controlWidth;
                }
                var rect = _panel.CreateRect(left, y, controlWidth, controlHeight);
                _panel.Arrange(control, rect);

                x = left;
            }

            return finalSize;
        }
    }
}
