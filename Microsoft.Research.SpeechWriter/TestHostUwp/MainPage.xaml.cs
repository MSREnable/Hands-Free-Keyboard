using Microsoft.Research.SpeechWriter.UI;
using Microsoft.Research.SpeechWriter.UI.Uwp;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestHostUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly ApplicationLayout<TileButton> _applicationLayout;

        public MainPage()
        {
            InitializeComponent();

            _applicationLayout = new ApplicationLayout<TileButton>(TheContent, 110, 0);
        }
    }
}
