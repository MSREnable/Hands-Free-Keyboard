using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Sensors;
using System.Windows;

namespace Microsoft.HandsFree.Keyboard
{
    /// <summary>
    /// Interaction logic for EulaWindow.xaml
    /// </summary>
    public partial class EulaWindow : Window
    {
        public EulaWindow()
        {
            InitializeComponent();
        }

        internal static void ShowDialogOnFirstRun()
        {
            if (!AppSettings.Instance.Keyboard.IsEulaAccepted)
            {
                var eulaWindow = new EulaWindow();
                eulaWindow.ShowDialog();
            }
        }

        // TODO: The Accept button should just set IsEulaAccepted true and be done. In order to
        // perform first-time configuration asynchronously in a sane manner, that is currently
        // being done as a side-effect of accepting the EULA. When the code is converted to use
        // the async model the CreateProfileAsync method will be removed.
        async void OnAcceptAsync(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToElementState(TheGrid, "ConfiguringSystem", true);

            var provider = GazeDataProvider.InitializeGazeDataProvider();
            var created = await provider.CreateProfileAsync();
            AppSettings.Instance.Keyboard.IsEulaAccepted = created;
            provider.Terminate();
            Close();
        }
    }
}
