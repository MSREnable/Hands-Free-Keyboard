using Microsoft.Research.SpeechWriter.Core;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface IApplicationPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        void ResetControls();

        TControl CreateControl(ITile tile);

        void DeleteControl(TControl control);

        TSize ToTSize(double width, double height);

        double WidthFromTSize(TSize size);

        double HeightFromTSize(TSize size);

        TRect ToTRect(double x, double y, TSize size);
    }
}