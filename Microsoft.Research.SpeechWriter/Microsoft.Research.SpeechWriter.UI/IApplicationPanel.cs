using Microsoft.Research.SpeechWriter.Core;
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

        TSize ToTSize(double width, double height);

        double WidthFromTSize(TSize size);

        double HeightFromTSize(TSize size);

        TRect ToTRect(double x, double y, TSize size);
    }
}