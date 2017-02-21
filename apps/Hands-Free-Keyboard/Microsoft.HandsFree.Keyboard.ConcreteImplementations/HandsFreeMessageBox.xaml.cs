using Microsoft.HandsFree.Mouse;
using Microsoft.HandsFree.Settings;
using System.Windows;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    /// <summary>
    /// Interaction logic for HandsFreeMessageBox.xaml
    /// </summary>
    public partial class HandsFreeMessageBox : Window
    {
        GazeMouse _mouse;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HandsFreeMessageBox()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                    TheTextBlock.Text = Message;
                    _mouse = GazeMouse.Attach(this, null, null, AppSettings.Instance.Mouse);
                };
        }

        /// <summary>
        /// The message to display.
        /// </summary>
        string Message { get; set; }

        internal static bool ShowMessage(Window owner, string text)
        {
            var window = new HandsFreeMessageBox
            {
                Message = text,
                Owner = owner
            };
            return window.ShowDialog().Value;
        }

        void OnYes(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        void OnNo(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
