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
        private readonly ApplicationLayout<ButtonUI> _applicationLayout;

        public MainWindow()
        {
            InitializeComponent();

            _applicationLayout = new ApplicationLayout<ButtonUI>(TheContent, 50, 5);
        }
    }
}
