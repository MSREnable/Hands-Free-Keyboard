namespace MultiPanel
{
    public interface IApplicationPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        TSize DesiredSize { get; }

        void Measure(TSize availableSize);
        void Arrange(TRect rect);

        double GetWidth(TSize desiredSize);
        double GetHeight(TSize desiredSize);
        TSize CreateSize(double width, double height);
        TRect CreateRect(double x, double y, double width, double height);
    }
}
