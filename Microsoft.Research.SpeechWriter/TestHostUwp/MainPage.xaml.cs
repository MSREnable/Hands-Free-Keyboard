using Microsoft.Research.SpeechWriter.Core;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestHostUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            TheOtherContent.Model = new ApplicationModel();
            TheOtherContent.SizeChanged += (s, e) => TheOtherContent.Model.MaxNextSuggestionsCount = (int)(e.NewSize.Height / 110);
        }
    }
}
