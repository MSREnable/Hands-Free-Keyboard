using System;
using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.Apps.Uwp
{
    public sealed partial class SwitchTargetControl : UserControl, INotifyPropertyChanged
    {
        public readonly DependencyProperty IndexProperty = DependencyProperty.Register(nameof(Index), typeof(int), typeof(SwitchTargetControl),
            new PropertyMetadata(0));
        public readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SwitchTargetControl),
            new PropertyMetadata(false, OnSelectedChanged));

        public readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register(nameof(HighlightColor), typeof(Brush), typeof(SwitchTargetControl),
            new PropertyMetadata(new SolidColorBrush(Colors.Orange)));

        public SwitchTargetControl()
        {
            this.InitializeComponent();
        }

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public Brush HighlightColor
        {
            get { return (Brush)GetValue(HighlightColorProperty); }
            set { SetValue(HighlightColorProperty, value); }
        }

        internal Action Action { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SwitchTargetControl)d;

            var brush = (bool)e.NewValue ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Orange);
            control.HighlightColor = brush;
            control.PropertyChanged?.Invoke(control, new PropertyChangedEventArgs(nameof(HighlightColor)));
        }
    }
}
