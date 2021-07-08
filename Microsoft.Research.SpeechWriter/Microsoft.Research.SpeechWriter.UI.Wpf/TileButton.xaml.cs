using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Data;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    /// <summary>
    /// Interaction logic for TileButton.xaml
    /// </summary>
    public partial class TileButton : UserControl
    {
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(ITile), typeof(TileButton),
            new PropertyMetadata(null, OnItemUpdated));

        public TileButton()
        {
            InitializeComponent();
        }

        public ITile Item
        {
            get => (ITile)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        private static void OnItemUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileButton)d).OnItemUpdated((ITile)e.NewValue);
        }

        private void OnItemUpdated(ITile tile)
        {
            TheStack.Children.Clear();

            TheButton.Command = tile;

            var visualization = tile.Visualization;

            switch (visualization.Type)
            {
                case TileVisualizationType.Normal: TheButton.Opacity = 1; break;
                case TileVisualizationType.Ghosted: TheButton.Opacity = 0.2; break;
                case TileVisualizationType.Hidden: TheButton.Opacity = 0; break;
            }

            foreach (var element in visualization.Elements)
            {
                UIElement child;

                switch (element.Type)
                {
                    case TileType.Command:
                        child = new TextControl { VisualizationElement = element };
                        break;

                    default:
                        child = new TileControl { VisualizationElement = element };
                        break;
                }

                TheStack.Children.Add(child);
            }
        }
    }
}
