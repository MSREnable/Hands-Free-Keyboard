using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
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
    public class SuperPanel : Panel, ISuperPanel<FrameworkElement, Size, Rect>
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(ApplicationModel), typeof(SuperPanel),
            new PropertyMetadata(null, OnModelChanged));

        private readonly SuperPanelHelper<FrameworkElement, Size, Rect> _helper;

        public SuperPanel()
        {
            _helper = new SuperPanelHelper<FrameworkElement, Size, Rect>(this);
        }

        void ISuperPanel<FrameworkElement, Size, Rect>.ResetChildren()
        {
            Children.Clear();
        }

        IApplicationPanel<FrameworkElement, Size, Rect> ISuperPanel<FrameworkElement, Size, Rect>.CreateChild(ApplicationPanelHelper<FrameworkElement, Size, Rect> helper)
        {
            var panel = new ApplicationPanel(helper);
            Children.Add(panel);
            return panel;
        }

        public ApplicationModel Model
        {
            get => (ApplicationModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (SuperPanel)d;
            var model = (ApplicationModel)e.NewValue;
            panel._helper.SetModel(model);
        }

        public Rect GetTargetRect(FrameworkElement target, ApplicationRobotAction action)
        {
            var rect = _helper.GetTargetRect(target, action);
            return rect;
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
