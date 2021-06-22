using Microsoft.Research.SpeechWriter.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    internal abstract class TileLayoutHelper<TControl, TSize, TRect> : LayoutHelper<TControl, TSize, TRect, ITile>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal TileLayoutHelper(ApplicationPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<ITile> list)
            : base(panel, list)
        {
        }

        protected override List<TControl> CreateControls(IEnumerable<ITile> list)
        {
            var controls = new List<TControl>();

            foreach (var item in list)
            {
                var control = _helper._panel.CreateControl(item);
                controls.Add(control);
            }

            return controls;
        }

        internal override TSize MeasureOverride(TSize availableSize)
        {
            return availableSize;
        }
    }
}
