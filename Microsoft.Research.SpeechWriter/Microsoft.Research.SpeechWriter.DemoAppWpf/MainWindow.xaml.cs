using Microsoft.Research.SpeechWriter.Core;
using System.Windows;

namespace Microsoft.Research.SpeechWriter.DemoAppWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApplicationModel _model = new ApplicationModel();

        public MainWindow()
        {
            DataContext = _model;

            InitializeComponent();

            TheContent.SizeChanged += MainWindow_SizeChanged;

            var vocalizer = new NarratorVocalizer();
            _ = Narrator.AttachNarrator(_model, vocalizer);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _model.MaxNextSuggestionsCount = (int)(e.NewSize.Height) / 60;
        }
    }
}
