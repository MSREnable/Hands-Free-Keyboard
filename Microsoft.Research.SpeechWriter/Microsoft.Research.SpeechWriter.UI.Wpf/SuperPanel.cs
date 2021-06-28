using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    public class SuperPanel : Panel, ISuperPanel<FrameworkElement, Size, Rect>
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(ApplicationModel), typeof(SuperPanel),
            new PropertyMetadata(null, OnModelChanged));

        IApplicationPanel<FrameworkElement, Size, Rect> ISuperPanel<FrameworkElement, Size, Rect>.CreateChild()
        {
            var panel = new ApplicationPanel();
            Children.Add(panel);
            return panel;
        }

        public ApplicationModel Model
        {
            get => (ApplicationModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public event EventHandler ModelChanged;

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (SuperPanel)d;
            panel.ModelChanged?.Invoke(d, EventArgs.Empty);
        }
    }
}
