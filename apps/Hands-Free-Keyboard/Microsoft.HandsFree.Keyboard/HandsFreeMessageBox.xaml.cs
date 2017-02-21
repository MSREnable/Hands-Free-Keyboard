using Microsoft.HandsFree.Mouse;
using Microsoft.HandsFree.Settings;
using System.Windows;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard
{
    /// <summary>
    /// Interaction logic for HandsFreeMessageBox.xaml
    /// </summary>
    public partial class HandsFreeMessageBox : Window
    {
        GazeMouse _mouse;

        public HandsFreeMessageBox()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                    TheTextBlock.Text = Message;
                    _mouse = GazeMouse.Attach(this, null, null, AppSettings.Instance.Mouse);
                };
        }

        public string Message { get; set; }

        internal static bool ShowMessage(Window owner, string text)
        {
            var window = new HandsFreeMessageBox()
            {
                Message = text
            };
            window.Owner = owner;
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
