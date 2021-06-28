using Microsoft.Research.SpeechWriter.Core;
using System;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface ISuperPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        ApplicationModel Model { get; set; }

        event EventHandler ModelChanged;

        IApplicationPanel<TControl, TSize, TRect> CreateChild();
    }
}
