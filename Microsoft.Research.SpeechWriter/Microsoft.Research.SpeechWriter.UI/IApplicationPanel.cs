using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface IApplicationPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        IEnumerable<TControl> Children { get; }

        void ResetControls();

        TControl CreateControl(ITile tile);

        TSize GetDesiredSize(TControl control);

        void Measure(TControl control, TSize availableSize);

        void Arrange(TControl control, TRect rect);

        void DeleteControl(TControl control);

        TRect GetTargetRect(TControl parent, ApplicationRobotAction action);

        TSize GetSize(double width, double height);

        double GetWidth(TSize size);

        double GetHeight(TSize size);

        TRect GetRect(double x, double y, TSize size);

        TRect GetRect(TControl parent, TControl control);
    }
}