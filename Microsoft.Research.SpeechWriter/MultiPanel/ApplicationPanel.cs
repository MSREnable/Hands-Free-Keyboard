using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MultiPanel
{
    public class ApplicationPanel : Panel, IApplicationPanel<FrameworkElement, Size, Rect>
    {
        private readonly PanelName _name;

        public ApplicationPanel(PanelName name)
        {
            _name = name;
        }

        double IApplicationPanel<FrameworkElement, Size, Rect>.GetWidth(Size size) => size.Width;
        double IApplicationPanel<FrameworkElement, Size, Rect>.GetHeight(Size size) => size.Height;
        Size IApplicationPanel<FrameworkElement, Size, Rect>.CreateSize(double width, double height) => new Size(width, height);
        Rect IApplicationPanel<FrameworkElement, Size, Rect>.CreateRect(double x, double y, double width, double height) => new Rect(x, y, width, height);

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = _name == PanelName.Interstitial ? new Size(110, 110 * Math.Floor(availableSize.Height / 110)) : availableSize;

            Debug.WriteLine($"ApplicationPanel.MeasureOverride {_name} {availableSize} => {size}");

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"ApplicationPanel.ArrangeOverride {_name} {finalSize}");

            return base.ArrangeOverride(finalSize);
        }
    }
}
