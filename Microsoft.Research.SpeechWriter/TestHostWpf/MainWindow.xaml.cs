using Microsoft.Research.SpeechWriter.Core.UI;
using Microsoft.Research.SpeechWriter.Core.UI.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace TestHostWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ApplicationLayout<ButtonUI> _applicationLayout;

        public MainWindow()
        {
            InitializeComponent();

            _applicationLayout = new ApplicationLayout<ButtonUI>(TheContent, 50, 5);
        }
    }
}
