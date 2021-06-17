using Microsoft.Research.SpeechWriter.Core;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    /// <summary>
    /// Interaction logic for TileButton.xaml
    /// </summary>
    public partial class TileButton : UserControl, IButtonUI
    {
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(ITile), typeof(TileButton));

        public TileButton()
        {
            InitializeComponent();
        }

        public ITile Item
        {
            get => (ITile)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        double IButtonUI.RenderedWidth => DesiredSize.Width;
    }
}
