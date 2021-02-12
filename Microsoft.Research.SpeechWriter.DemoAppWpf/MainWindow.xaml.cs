﻿using Microsoft.Research.RankWriter.Library;
using Microsoft.Research.RankWriter.Library.Items;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows;

namespace Microsoft.Research.RankWriter.AltTestHost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApplicationModel _model = new ApplicationModel();

        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        public MainWindow()
        {
            DataContext = _model;

            InitializeComponent();

            TheContent.SizeChanged += MainWindow_SizeChanged;

            ((INotifyCollectionChanged)_model.SelectedItems).CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (HeadWordItem item in e.NewItems)
                {
                    Debug.WriteLine(item);

                    _synthesizer.SpeakAsync(item.ToString());
                }
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _model.MaxNextSuggestionsCount = (int)(e.NewSize.Height) / 60;
        }
    }
}
