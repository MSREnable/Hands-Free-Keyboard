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

        internal SuggestionsLayoutHelper(SuperPanelHelper<TControl, TSize, TRect> panel,
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
                    var control = _panel.CreateControl(item);
                    controls.Add(control);
                    subGroupedControls.Add(control);
                }
                groupedControls.Add(subGroupedControls);
            }

            _groupedControls = groupedControls;

            return controls;
        }

        public override TSize ArrangeOverride(TSize finalSize)
        {
            var y = 0.0;
            foreach (var subGroup in _groupedControls)
            {
                var x = 0.0;

                var controlHeight = 0.0;

                foreach (var control in subGroup)
                {
                    var controlSize = _panel.GetDesiredSize(control);
                    var controlWidth = _panel.GetWidth(controlSize);
                    /*var*/
                    controlHeight = _panel.GetHeight(controlSize);
                    var rect = _panel.CreateRect(x, y, controlWidth, controlHeight);
                    _panel.Arrange(control, rect);

                    x += controlWidth;
                }

                y += controlHeight;
            }

            return finalSize;
        }

        internal TControl GetControl(int index, int subIndex)
        {
            return _groupedControls[index][subIndex];
        }
    }
}
