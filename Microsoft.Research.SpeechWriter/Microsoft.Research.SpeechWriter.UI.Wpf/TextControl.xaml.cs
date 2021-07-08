using Microsoft.Research.SpeechWriter.Core.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TextControl : UserControl
    {
        public static readonly DependencyProperty VisualizationElementProperty = DependencyProperty.Register(nameof(TileVisualizationElement), typeof(TileVisualizationElement), typeof(TextControl),
            new PropertyMetadata(null, OnVisualizationElementChanged));

        private static readonly FontFamily _normal = new FontFamily("Segoe UI");
        private static readonly FontFamily _special = new FontFamily("Segoe MDL2 Assets");

        public TextControl()
        {
            InitializeComponent();
        }

        public TileVisualizationElement VisualizationElement
        {
            get => (TileVisualizationElement)GetValue(VisualizationElementProperty);
            set => SetValue(VisualizationElementProperty, value);
        }

        private void OnVisualizationElementChanged(TileVisualizationElement element)
        {
            TheTextBlock.Text = element.Text;
            TheTextBlock.Foreground = element.Foreground.ToBrush();
            FontFamily = element.Text == "\xE775" ? _special : _normal;
        }

        private static void OnVisualizationElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextControl)d).OnVisualizationElementChanged((TileVisualizationElement)e.NewValue);
        }
    }
}
