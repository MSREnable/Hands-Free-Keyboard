using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Items;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public sealed partial class TileButton : UserControl
    {
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(ITile), typeof(TileButton),
            new PropertyMetadata(null, OnItemChanged));

        private static readonly ButtonResourceHelper _helper = new ButtonResourceHelper();

        public TileButton()
        {
            this.InitializeComponent();
        }

        public ITile Item
        {
            get => (ITile)GetValue(ItemProperty);
            set
            {
                SetValue(ItemProperty, value);
            }
        }

        private void OnItemChanged(object value)
        {
            if (value is InterstitialNonItem)
            {
                Opacity = 0.0;
            }
            else if (value is GhostWordItem || value is GhostStopItem)
            {
                Opacity = 0.2;
            }
            else
            {
                Opacity = 1.0;
            }

            TheButton.ContentTemplate = _helper.GetTemplate(value);
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileButton)d).OnItemChanged(e.NewValue);
        }

        internal void Recycle()
        {
            // TODO: Will bring UWP into line with WPF later.
        }
    }
}
