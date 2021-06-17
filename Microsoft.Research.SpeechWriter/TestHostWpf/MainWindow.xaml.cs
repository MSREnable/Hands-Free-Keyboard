using Microsoft.Research.SpeechWriter.UI;
using Microsoft.Research.SpeechWriter.UI.Wpf;
using System.Windows;

namespace TestHostWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApplicationLayout<TileButton> _applicationLayout;

        public MainWindow()
        {
            InitializeComponent();

            _applicationLayout = new ApplicationLayout<TileButton>(TheContent, 110);
        }
    }
}
