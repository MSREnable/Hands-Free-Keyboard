using Microsoft.Research.SpeechWriter.Core.UI;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestHostUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly ApplicationLayout<ButtonUI> _applicationLayout;

        public MainPage()
        {
            InitializeComponent();

            _applicationLayout = new ApplicationLayout<ButtonUI>(TheContent, 50, 5);
        }
    }
}
