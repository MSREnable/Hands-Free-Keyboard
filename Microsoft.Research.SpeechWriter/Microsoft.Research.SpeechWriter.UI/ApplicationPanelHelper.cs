namespace Microsoft.Research.SpeechWriter.UI
{
    public abstract class ApplicationPanelHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        internal readonly IApplicationPanel<TControl, TSize, TRect> _panel;

        public ApplicationPanelHelper(SuperPanelHelper<TControl, TSize, TRect> helper)
        {
            _panel = helper.CreateChild(this);
        }

        public TSize MeasureOverride(TSize availableSize)
        {
            foreach(var control in _panel.Children)
            {
                _panel.Measure(control, availableSize);
            }

            return availableSize;
        }

        public abstract TSize ArrangeOverride(TSize finalSize);
    }
}
