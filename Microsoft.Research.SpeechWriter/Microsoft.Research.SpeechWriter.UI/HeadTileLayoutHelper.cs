using Microsoft.Research.SpeechWriter.Core;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    internal class HeadTileLayoutHelper<TControl, TSize, TRect> : TileLayoutHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal HeadTileLayoutHelper(ApplicationPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<ITile> list)
            : base(panel, list)
        {
        }

        internal override void Arrange()
        {
            var x = _helper.HeadLeft;
            var y = _helper.HeadTop;

            foreach (var control in _controls)
            {
                var controlSize = _helper._panel.GetDesiredSize(control);
                var right = x + _helper._panel.WidthFromTSize(controlSize);
                if (_helper.HeadRight < right && x != _helper.HeadLeft)
                {
                    x = _helper.HeadLeft;
                    y += _helper.Pitch;
                }
                var rect = _helper._panel.ToTRect(x, y, controlSize);
                _helper._panel.Arrange(control, rect);

                x += _helper._panel.WidthFromTSize(controlSize);
            }
        }
    }
}
