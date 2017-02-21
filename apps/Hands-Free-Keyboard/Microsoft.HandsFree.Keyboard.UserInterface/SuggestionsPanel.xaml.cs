using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.MVVM;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.HandsFree.Keyboard.UserInterface
{
    /// <summary>
    /// Interaction logic for AugmentedKeyboard.xaml
    /// </summary>
    public partial class SuggestionPanel : UserControl
    {
        public SuggestionPanel()
        {
            InitializeComponent();

            SetVisualState();
            AppSettings.Instance.Prediction.AttachPropertyChangedAction(nameof(AppSettings.Instance.Prediction.PredictionLayout), SetVisualState);
        }

        void SetVisualState()
        {
            var layout = AppSettings.Instance.Prediction.PredictionLayout;
            var stateName = layout.ToString();
            VisualStateManager.GoToElementState(ThePanel, stateName, false);
        }
    }
}
