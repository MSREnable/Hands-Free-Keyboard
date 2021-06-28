using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface ISuperPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        ApplicationModel Model { get; set; }

        IApplicationPanel<TControl, TSize, TRect> CreateChild();

        TRect GetTargetRect(TControl control, ApplicationRobotAction action);
    }
}
