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
        internal TailTileLayoutHelper(ApplicationPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<ITile> list)
            : base(panel, list)
        {
        }

        internal override void Arrange()
        {
            var reversedControls = new List<TControl>(_controls);
            reversedControls.Reverse();

            var x = _helper.TailRight;
            var y = _helper.TailTop;

            foreach (var control in reversedControls)
            {
                var controlSize = _helper._panel.GetDesiredSize(control);
                var left = x - _helper._panel.GetWidth(controlSize);
                if (left < _helper.TailLeft && x != _helper.HeadRight)
                {
                    x = _helper.TailRight;
                    y -= _helper.Pitch;
                    left = x - _helper._panel.GetWidth(controlSize);
                }
                var rect = _helper._panel.CreateRect(left, y, controlSize);
                _helper._panel.Arrange(control, rect);

                x = left;
            }
        }
    }
}
