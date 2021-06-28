using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiPanel
{
    public class SuperPanel : Panel, ISuperPanel<FrameworkElement, Size, Rect>
    {
        private readonly SuperPanelHelper<FrameworkElement, Size, Rect> _helper;

        public SuperPanel()
        {
            _helper = new SuperPanelHelper<FrameworkElement, Size, Rect>(this);
        }

        IApplicationPanel<FrameworkElement, Size, Rect> ISuperPanel<FrameworkElement, Size, Rect>.CreateChild()
        {
            var panelName = (PanelName)Children.Count;
            var brush = (new[] { Brushes.Red, Brushes.Green, Brushes.Orange, Brushes.Blue })[Children.Count];
            var panel = new ApplicationPanel(panelName) { Background = brush };

            Children.Add(panel);

            return panel;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return _helper.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return _helper.ArrangeOverride(finalSize);
        }
    }
}
