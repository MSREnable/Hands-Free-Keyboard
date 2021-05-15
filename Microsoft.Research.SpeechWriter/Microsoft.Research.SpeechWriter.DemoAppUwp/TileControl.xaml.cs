using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    public sealed partial class TileControl : UserControl
    {
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(nameof(Caption), typeof(string), typeof(TileControl),
            new PropertyMetadata(null, UpdateBorder));
        public static readonly DependencyProperty ShowAttachedToPreviousProperty = DependencyProperty.Register(nameof(ShowAttachedToPrevious), typeof(bool), typeof(TileControl),
            new PropertyMetadata(false, UpdateBorder));
        public static readonly DependencyProperty ShowAttachedToNextProperty = DependencyProperty.Register(nameof(ShowAttachedToNext), typeof(bool), typeof(TileControl),
            new PropertyMetadata(false, UpdateBorder));
        public static readonly DependencyProperty BorderProperty = DependencyProperty.Register(nameof(Border), typeof(Thickness), typeof(TileControl),
            new PropertyMetadata(new Thickness(1), UpdateBorder));
        public static readonly DependencyProperty InternalBorderProperty = DependencyProperty.Register(nameof(InternalBorder), typeof(Thickness), typeof(TileControl),
            new PropertyMetadata(new Thickness(1)));

        public TileControl()
        {
            this.InitializeComponent();
        }

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public bool ShowAttachedToPrevious
        {
            get => (bool)GetValue(ShowAttachedToPreviousProperty);
            set => SetValue(ShowAttachedToPreviousProperty, value);
        }

        public bool ShowAttachedToNext
        {
            get => (bool)GetValue(ShowAttachedToNextProperty);
            set => SetValue(ShowAttachedToNextProperty, value);
        }

        public Thickness Border
        {
            get => (Thickness)GetValue(BorderProperty);
            set => SetValue(BorderProperty, value);
        }

        public Thickness InternalBorder
        {
            get => (Thickness)GetValue(InternalBorderProperty);
            set => SetValue(InternalBorderProperty, value);
        }

        private void UpdateBorder(DependencyPropertyChangedEventArgs e)
        {
            var border = Border;
            var showAttachedToPrevious = ShowAttachedToPrevious;
            var showAttachedToNext = ShowAttachedToNext;

            var internalBorder = new Thickness(left: showAttachedToPrevious ? 0 : border.Left,
                right: showAttachedToNext ? 0 : border.Right, top: border.Top, bottom: border.Bottom);
            InternalBorder = internalBorder;
        }

        private static void UpdateBorder(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileControl)d).UpdateBorder(e);
        }
    }
}
