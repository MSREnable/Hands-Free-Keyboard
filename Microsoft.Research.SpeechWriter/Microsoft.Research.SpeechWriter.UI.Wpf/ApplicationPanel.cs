using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

#if WINDOWS_UWP
namespace Microsoft.Research.SpeechWriter.UI.Uwp
#else
namespace Microsoft.Research.SpeechWriter.UI.Wpf
#endif
{
    public class ApplicationPanel : Panel, IApplicationPanel<FrameworkElement, Size, Rect>
    {
        private readonly ApplicationPanelHelper<FrameworkElement, Size, Rect> _helper;

        public ApplicationPanel(ApplicationPanelHelper<FrameworkElement, Size, Rect> helper)
        {
            _helper = helper;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = _helper.MeasureOverride(availableSize);
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = _helper.ArrangeOverride(finalSize);
            return size;
        }

        void IApplicationPanel<FrameworkElement, Size, Rect>.ResetControls()
        {
            Children.Clear();
        }

        IEnumerable<FrameworkElement> IApplicationPanel<FrameworkElement, Size, Rect>.Children
        {
            get
            {
                foreach (FrameworkElement control in Children)
                {
                    yield return control;
                }
            }
        }

        FrameworkElement IApplicationPanel<FrameworkElement, Size, Rect>.CreateControl(ITile tile)
        {
            var button = new TileButton { Item = tile };
            Children.Add(button);
            return button;
        }

        void IApplicationPanel<FrameworkElement, Size, Rect>.Measure(FrameworkElement control, Size availableSize)
        {
            control.Measure(availableSize);
        }

        Size IApplicationPanel<FrameworkElement, Size, Rect>.GetDesiredSize(FrameworkElement control)
        {
            return control.DesiredSize;
        }

        void IApplicationPanel<FrameworkElement, Size, Rect>.Arrange(FrameworkElement control, Rect rect)
        {
            control.Arrange(rect);
        }

        public Rect GetTargetRect(FrameworkElement parent, ApplicationRobotAction action)
        {
            return _helper.GetTargetRect(parent, action);
        }

        void IApplicationPanel<FrameworkElement, Size, Rect>.DeleteControl(FrameworkElement control)
        {
            Children.Remove(control);
        }

        Size IApplicationPanel<FrameworkElement, Size, Rect>.CreateSize(double width, double height)
        {
            return new Size(width, height);
        }

        double IApplicationPanel<FrameworkElement, Size, Rect>.GetWidth(Size size)
        {
            return size.Width;
        }

        double IApplicationPanel<FrameworkElement, Size, Rect>.GetHeight(Size size)
        {
            return size.Height;
        }

        Rect IApplicationPanel<FrameworkElement, Size, Rect>.CreateRect(double x, double y, Size size)
        {
            return new Rect(x, y, size.Width, size.Height);
        }

        Rect IApplicationPanel<FrameworkElement, Size, Rect>.CreateRect(double x, double y, double width, double height)
        {
            return new Rect(x, y, width, height);
        }

        Rect IApplicationPanel<FrameworkElement, Size, Rect>.CreateRect(FrameworkElement parent, FrameworkElement control)
        {
            var transform = control.TransformToVisual(parent);
            var sourceRect = new Rect(new Point(0, 0), new Point(control.ActualWidth, control.ActualHeight));
            var targetRect = transform.TransformBounds(sourceRect);
            return targetRect;
        }
    }
}
