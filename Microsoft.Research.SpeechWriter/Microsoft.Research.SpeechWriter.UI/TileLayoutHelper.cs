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
        internal TileLayoutHelper(SuperPanelHelper<TControl, TSize, TRect> superHelper,
            ReadOnlyObservableCollection<ITile> list)
            : base(superHelper, list)
        {
        }

        protected override void AddContent(IList<ITile> list, int startIndex, int count)
        {
            if (startIndex == _controls.Count)
            {
                for (var i = 0; i < count; i++)
                {
                    var control = _panel.CreateControl(list[startIndex + i].Visualization);
                    _controls.Add(control);
                }
            }
            else
            {
                ResetContent(list);
            }
        }

        protected override void RemoveContent(IList<ITile> list, int startIndex, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var oldControl = _controls[startIndex];
                _controls.RemoveAt(startIndex);
                _panel.DeleteControl(oldControl);
            }
        }

        protected override void ReplaceContent(IList<ITile> list, int startIndex, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var oldControl = _controls[startIndex + i];
                var newControl = _panel.CreateControl(list[startIndex + i].Visualization);

                _controls[startIndex + i] = newControl;
                _panel.DeleteControl(oldControl);
            }
        }

        protected override List<TControl> CreateControls(IEnumerable<ITile> list)
        {
            var controls = new List<TControl>();

            foreach (var item in list)
            {
                var control = _panel.CreateControl(item.Visualization);
                controls.Add(control);
            }

            return controls;
        }

        internal TControl GetControl(int index)
        {
            return _controls[index];
        }
    }
}
