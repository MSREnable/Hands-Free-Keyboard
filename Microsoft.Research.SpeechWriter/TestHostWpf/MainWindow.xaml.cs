using Microsoft.Research.SpeechWriter.Core;
using System.Windows;

namespace TestHostWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TheOtherContent.Model = new ApplicationModel();
            TheOtherContent.SizeChanged += (s, e) => TheOtherContent.Model.MaxNextSuggestionsCount = (int)(e.NewSize.Height / 110);
        }
    }
}
