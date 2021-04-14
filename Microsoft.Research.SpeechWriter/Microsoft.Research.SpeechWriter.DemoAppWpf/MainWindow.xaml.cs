using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Items;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows;

namespace Microsoft.Research.SpeechWriter.DemoAppWpf
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

            ((INotifyCollectionChanged)_model.HeadItems).CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (HeadWordItem item in e.NewItems)
                {
                    Debug.WriteLine(item);

                    _synthesizer.SpeakAsync(item.Content);
                }
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _model.MaxNextSuggestionsCount = (int)(e.NewSize.Height) / 60;
        }
    }
}
