using Microsoft.Research.SpeechWriter.Core;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    internal class InterstitialTileLayoutHelper<TControl, TSize, TRect> : TileLayoutHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal InterstitialTileLayoutHelper(ApplicationPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<ITile> list)
            : base(panel, list)
        {
        }

        internal override void Arrange()
        {
            var x = _helper.InterstitialLeft;
            var y = _helper.InterstitialTop;

            foreach(var control in _controls)
            {
                var controlSize = _helper._panel.GetDesiredSize(control);
                var rect = _helper._panel.ToTRect(x, y, controlSize);
                _helper._panel.Arrange(control, rect);

                y += _helper._panel.HeightFromTSize(controlSize);
            }
        }
    }
}
