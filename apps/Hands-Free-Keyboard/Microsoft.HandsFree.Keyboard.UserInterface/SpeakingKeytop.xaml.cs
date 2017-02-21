using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.HandsFree.MVVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.UserInterface
{
    /// <summary>
    /// Interaction logic for SpeakingKeytop.xaml
    /// </summary>
    public partial class SpeakingKeytop : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SpeakingKeytop));

        public static readonly DependencyProperty KeytopProperty = DependencyProperty.Register(nameof(Keytop), typeof(string), typeof(SpeakingKeytop));

        public static readonly DependencyProperty RepeatMultiplierProperty = DependencyProperty.Register(nameof(RepeatMultiplier), typeof(double), typeof(SpeakingKeytop));

        public SpeakingKeytop()
        {
            Speak = new RelayCommand(OnSpeak);

            InitializeComponent();
        }

        public ICommand Speak { get; private set; }

        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        public string Keytop { get { return (string)GetValue(KeytopProperty); } set { SetValue(KeytopProperty, value); } }

        public double RepeatMultiplier { get { return (double)GetValue(RepeatMultiplierProperty); } set { SetValue(RepeatMultiplierProperty, value); } }

        void OnSpeak(object o)
        {
            var host = (IKeyboardHost)DataContext;
            host.SpeakFixedText(Text ?? Keytop);
        }
    }
}
