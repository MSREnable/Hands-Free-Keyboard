using Microsoft.Research.SpeechWriter.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    internal class SuggestionsLayoutHelper<TControl, TSize, TRect> : LayoutHelper<TControl, TSize, TRect, IEnumerable<ITile>>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        private List<List<TControl>> _groupedControls;

        internal SuggestionsLayoutHelper(ApplicationPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<IEnumerable<ITile>> list)
            : base(panel, list)
        {
        }

        protected override List<TControl> CreateControls(IEnumerable<IEnumerable<ITile>> list)
        {
            var controls = new List<TControl>();

            var groupedControls = new List<List<TControl>>();

            foreach (var subList in list)
            {
                var subGroupedControls = new List<TControl>();

                foreach (var item in subList)
                {
                    var control = _helper._panel.CreateControl(item);
                    controls.Add(control);
                    subGroupedControls.Add(control);
                }
                groupedControls.Add(subGroupedControls);
            }

            _groupedControls = groupedControls;

            return controls;
        }

        internal override TSize MeasureOverride(TSize availableSize)
        {
            return availableSize;
        }

        internal override void Arrange()
        {
            var y = _helper.SuggestionsTop;
            foreach (var subGroup in _groupedControls)
            {
                var x = _helper.SuggestionsLeft;

                foreach (var control in subGroup)
                {
                    var controlSize = _helper._panel.GetDesiredSize(control);
                    var rect = _helper._panel.ToTRect(x, y, controlSize);
                    _helper._panel.Arrange(control, rect);

                    x += _helper._panel.WidthFromTSize(controlSize);
                }

                y += _helper.Pitch;
            }
        }

        internal TControl GetControl(int index, int subIndex)
        {
            return _groupedControls[index][subIndex];
        }
    }
}
