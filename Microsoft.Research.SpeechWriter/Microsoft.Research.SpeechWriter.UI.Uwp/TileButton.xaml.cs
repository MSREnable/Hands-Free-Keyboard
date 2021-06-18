using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Items;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public sealed partial class TileButton : UserControl, IButtonUI
    {
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(ITile), typeof(TileButton),
            new PropertyMetadata(null, OnItemChanged));

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
                Debug.WriteLine($"Binding: {value}");
            }
        }

        double IButtonUI.RenderedWidth => DesiredSize.Width;

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

            var typeName = value.GetType().Name;

            var resources = (IEnumerable<KeyValuePair<object, object>>)Resources;

            using (var enumerator = resources.GetEnumerator())
            {
                DataTemplate template = null;

                while (template == null && enumerator.MoveNext())
                {
                    if (Equals(enumerator.Current.Key, typeName))
                    {
                        template = enumerator.Current.Value as DataTemplate;
                    }
                }

                TheButton.ContentTemplate = template;
            }
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileButton)d).OnItemChanged(e.NewValue);
        }
    }
}
