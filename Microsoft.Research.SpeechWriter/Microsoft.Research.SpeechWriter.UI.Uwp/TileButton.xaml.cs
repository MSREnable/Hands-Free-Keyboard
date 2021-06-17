﻿using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public sealed partial class TileButton : UserControl
    {
        public readonly static DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(object), typeof(TileButton),
            new PropertyMetadata(null, OnItemChanged));

        public TileButton()
        {
            this.InitializeComponent();
        }

        public object Item
        {
            get => (object)GetValue(ItemProperty);
            set
            {
                SetValue(ItemProperty, value);
                Debug.WriteLine($"Binding: {value}");
            }
        }

        private void OnItemChanged(object value)
        {
            try
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
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TileButton)d).OnItemChanged(e.NewValue);
        }
    }
}
