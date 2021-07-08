using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public sealed partial class TileControl : UserControl
    {
        public static readonly DependencyProperty VisualizationElementProperty = DependencyProperty.Register(nameof(TileVisualizationElement), typeof(TileVisualizationElement), typeof(TileControl),
            new PropertyMetadata(null, OnVisualizationElementChanged));

        public static readonly DependencyProperty BorderProperty = DependencyProperty.Register(nameof(Border), typeof(Thickness), typeof(TileControl),
            new PropertyMetadata(new Thickness(1)));

        public TileControl()
        {
            InitializeComponent();
        }

        public TileVisualizationElement VisualizationElement
        {
            get => (TileVisualizationElement)GetValue(VisualizationElementProperty);
            set => SetValue(VisualizationElementProperty, value);
        }

        public Thickness Border
        {
            get => (Thickness)GetValue(BorderProperty);
            set => SetValue(BorderProperty, value);
        }

        private void OnVisualizationElementChanged(TileVisualizationElement element)
        {
            var border = Border;
            var isSuffix = element.Type == TileType.Suffix || element.Type == TileType.Infix;
            var isPrefix = element.Type == TileType.Prefix || element.Type == TileType.Infix;
            TheBorder.BorderThickness = new Thickness(left: isSuffix ? 0 : border.Left,
                right: isPrefix ? 0 : border.Right, top: border.Top, bottom: border.Bottom); ;
            TheBorder.Background = element.Background.ToBrush();
            TheTextBlock.Text = element.Text;
        }

        private static void OnVisualizationElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileControl)d).OnVisualizationElementChanged((TileVisualizationElement)e.NewValue);
        }
    }
}
