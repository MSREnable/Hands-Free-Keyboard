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
    public class BasePanel : Panel, IPanel<FrameworkElement, Size, Rect>
    {
        double IPanel<FrameworkElement, Size, Rect>.GetWidth(Size size)
        {
            return size.Width;
        }

        double IPanel<FrameworkElement, Size, Rect>.GetHeight(Size size)
        {
            return size.Height;
        }

        Size IPanel<FrameworkElement, Size, Rect>.CreateSize(double width, double height)
        {
            return new Size(width, height);
        }

        Rect IPanel<FrameworkElement, Size, Rect>.CreateRect(double x, double y, double width, double height)
        {
            return new Rect(x, y, width, height);
        }

        Rect IPanel<FrameworkElement, Size, Rect>.CreateRect(FrameworkElement parent, FrameworkElement control)
        {
            var transform = control.TransformToVisual(parent);
            var sourceRect = new Rect(new Point(0, 0), new Point(control.ActualWidth, control.ActualHeight));
            var targetRect = transform.TransformBounds(sourceRect);
            return targetRect;
        }
    }
}
