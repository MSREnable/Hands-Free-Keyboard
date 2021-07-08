using Microsoft.Research.SpeechWriter.Core;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface IApplicationPanel<TControl, TSize, TRect> : IPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        TSize DesiredSize { get; }
        void Measure(TSize availableSize);
        void Arrange(TRect finalRect);

        IEnumerable<TControl> Children { get; }

        void ResetControls();

        TControl CreateControl(TileVisualization tile);

        TSize GetDesiredSize(TControl control);

        void Measure(TControl control, TSize availableSize);

        void Arrange(TControl control, TRect rect);

        void DeleteControl(TControl control);
    }
}