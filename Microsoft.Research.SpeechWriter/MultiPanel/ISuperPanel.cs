namespace MultiPanel
{
    public interface ISuperPanel<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        IApplicationPanel<TControl, TSize, TRect> CreateChild();
    }
}
