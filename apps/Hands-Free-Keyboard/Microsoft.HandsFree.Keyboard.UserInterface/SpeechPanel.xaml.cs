using System.Windows.Controls;

namespace Microsoft.HandsFree.Keyboard.UserInterface
{
    /// <summary>
    /// Interaction logic for SpeachPanel.xaml
    /// </summary>
    public partial class SpeechPanel : UserControl
    {
        public SpeechPanel()
        {
            InitializeComponent();
        }

        public TextBox TextBox { get { return TheTextBox; } }
    }
}
