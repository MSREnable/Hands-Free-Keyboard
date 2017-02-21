using Microsoft.HandsFree.Keyboard.ConcreteImplementations;
using Microsoft.HandsFree.Mouse;
using System.Windows;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.Special
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly KeyboardApplicationEnvironment _environment;

        static readonly GazeClickParameters GazeClickParameters = new GazeClickParameters
        {
            MouseDownDelay = 250,
            MouseUpDelay = 500,
            RepeatMouseDownDelay = uint.MaxValue
        };

        public MainWindow()
        {
            var gazeMouseSettings = AppSettings.Instance.Mouse;
            _environment = KeyboardApplicationEnvironment.Create(this, GetGazeClickParameters);

            InitializeComponent();
        }

        GazeClickParameters GetGazeClickParameters(FrameworkElement element)
        {
            return GazeClickParameters;
        }
    }
}
