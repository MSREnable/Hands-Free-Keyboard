using Microsoft.Research.SpeechWriter.Core;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    internal class HeadTileLayoutHelper<TControl, TSize, TRect> : TileLayoutHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal HeadTileLayoutHelper(SuperPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<ITile> list)
            : base(panel, list)
        {
        }

        public override TSize ArrangeOverride(TSize finalSize)
        {
            var panelWidth = _panel.GetWidth(finalSize);

            var x = 0.0;
            var y = 0.0;

            foreach (var control in _controls)
            {
                var controlSize = _panel.GetDesiredSize(control);
                var controlWidth = _panel.GetWidth(controlSize);
                var controlHeight = _panel.GetHeight(controlSize);

                var controlRight = x + controlWidth;
                if (panelWidth < controlRight && x != 0.0)
                {
                    x = 0;
                    y += controlHeight;
                }

                var rect = _panel.CreateRect(x, y, controlWidth, controlHeight);
                _panel.Arrange(control, rect);

                x += controlWidth;
            }

            return finalSize;
        }
    }
}
