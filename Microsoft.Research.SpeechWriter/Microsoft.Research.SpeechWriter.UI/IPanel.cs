namespace Microsoft.Research.SpeechWriter.UI
{
    public interface IPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        double GetWidth(TSize size);

        double GetHeight(TSize size);

        TSize CreateSize(double width, double height);

        TRect CreateRect(TControl parent, TControl control);
        TRect CreateRect(double x, double y, double width, double height);
    }
}
