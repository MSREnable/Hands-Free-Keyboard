using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Research.SpeechWriter.UI
{
    public abstract class ApplicationPanelHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal readonly IApplicationPanel<TControl, TSize, TRect> _panel;

        //private ApplicationModel _model;

        public ApplicationPanelHelper(SuperPanelHelper<TControl, TSize, TRect> helper)
        {
            _panel = helper.CreateChild(this);
        }

        public double Pitch { get; set; } = 110;

        public TSize MeasureOverride(TSize availableSize)
        {
            foreach(var control in _panel.Children)
            {
                _panel.Measure(control, availableSize);
            }

            return availableSize;
        }

        public abstract TSize ArrangeOverride(TSize finalSize);

        public TRect GetTargetRect(TControl parent, ApplicationRobotAction action)
        {
            TControl control = _panel.Children.First();

            var rect = _panel.CreateRect(parent, control);

            return rect;
        }
    }
}
