using Microsoft.Research.SpeechWriter.Core;
using System.Windows;

namespace Microsoft.Research.SpeechWriter.Apps.Wpf
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog(WriterSettings settings)
        {
            InitializeComponent();

            DataContext = settings;
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
