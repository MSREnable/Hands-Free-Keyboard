using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TileControl : UserControl
    {
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(nameof(Caption), typeof(string), typeof(TileControl),
            new PropertyMetadata(null, UpdateBorder));
        public static readonly DependencyProperty ShowAttachedToPreviousProperty = DependencyProperty.Register(nameof(IsSuffix), typeof(bool), typeof(TileControl),
            new PropertyMetadata(false, UpdateBorder));
        public static readonly DependencyProperty ShowAttachedToNextProperty = DependencyProperty.Register(nameof(IsPrefix), typeof(bool), typeof(TileControl),
            new PropertyMetadata(false, UpdateBorder));
        public static readonly DependencyProperty BorderProperty = DependencyProperty.Register(nameof(Border), typeof(Thickness), typeof(TileControl),
            new PropertyMetadata(new Thickness(1), UpdateBorder));
        public static readonly DependencyProperty InternalBorderProperty = DependencyProperty.Register(nameof(InternalBorder), typeof(Thickness), typeof(TileControl),
            new PropertyMetadata(new Thickness(1)));

        public TileControl()
        {
            InitializeComponent();
        }

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public bool IsSuffix
        {
            get => (bool)GetValue(ShowAttachedToPreviousProperty);
            set => SetValue(ShowAttachedToPreviousProperty, value);
        }

        public bool IsPrefix
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
            var isSuffix = IsSuffix;
            var isPrefix = IsPrefix;

            var internalBorder = new Thickness(left: isSuffix ? 0 : border.Left,
                right: isPrefix ? 0 : border.Right, top: border.Top, bottom: border.Bottom);
            InternalBorder = internalBorder;
        }

        private static void UpdateBorder(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileControl)d).UpdateBorder(e);
        }
    }
}
