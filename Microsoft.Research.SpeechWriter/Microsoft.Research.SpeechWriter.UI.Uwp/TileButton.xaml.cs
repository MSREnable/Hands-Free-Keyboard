using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Data;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public sealed partial class TileButton : UserControl
    {
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(TileVisualization), typeof(TileButton),
            new PropertyMetadata(null, OnItemUpdated));

        public TileButton()
        {
            this.InitializeComponent();
        }

        public TileVisualization Item
        {
            get => (TileVisualization)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        private static void OnItemUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileButton)d).OnItemUpdated((TileVisualization)e.NewValue);
        }

        private void OnItemUpdated(TileVisualization tile)
        {
            Debug.Assert(TheStack.Children.Count == 0);

            TheButton.Command = tile.Command;

            var visualization = tile;

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
                        var textControl = RecyclingFactory<TextControl>.Create();
                        textControl.VisualizationElement = element;
                        child = textControl;
                        break;

                    default:
                        var tileControl = RecyclingFactory.Create<TileControl>();
                        tileControl.VisualizationElement = element;
                        child = tileControl;
                        break;
                }

                TheStack.Children.Add(child);
            }
        }

        internal void Recycle()
        {
            foreach (var child in TheStack.Children)
            {
                var tileControl = child as TileControl;
                if (tileControl != null)
                {
                    RecyclingFactory.Recycle(tileControl);
                }
                else
                {
                    RecyclingFactory.Recycle((TextControl)child);
                }
            }
            TheStack.Children.Clear();

            RecyclingFactory.Recycle(this);
        }
    }
}
