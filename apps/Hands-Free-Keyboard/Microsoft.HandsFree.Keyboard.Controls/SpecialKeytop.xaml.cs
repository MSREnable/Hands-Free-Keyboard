using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Interaction logic for AlphanumericKeytop.xaml
    /// </summary>
    public sealed partial class SpecialKeytop : UserControl, IHostedControl
    {
        /// <summary>
        /// Keytop caption property.
        /// </summary>
        public static readonly DependencyProperty KeytopProperty = DependencyProperty.Register(nameof(Keytop), typeof(string), typeof(SpecialKeytop));

        /// <summary>
        /// Keytop caption property.
        /// </summary>
        public static readonly DependencyProperty SizedKeytopProperty = DependencyProperty.Register(nameof(SizedKeytop), typeof(string), typeof(SpecialKeytop));

        /// <summary>
        /// Keytop command property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SpecialKeytop));

        /// <summary>
        /// Activation delay multiplier property.
        /// </summary>
        public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(nameof(Multiplier), typeof(double), typeof(SpecialKeytop),
            new PropertyMetadata(1.0));

        /// <summary>
        /// Multiplier applied to repetition time calculation property.
        /// </summary>
        public static readonly DependencyProperty RepeatMultiplierProperty = DependencyProperty.Register(nameof(RepeatMultiplier), typeof(double), typeof(SpecialKeytop),
            new PropertyMetadata(0.0));

        /// <summary>
        /// Vocalisation property.
        /// </summary>
        public static readonly DependencyProperty VocalProperty = DependencyProperty.Register(nameof(Vocal), typeof(string), typeof(SpecialKeytop));

        /// <summary>
        /// Is transparent background property.
        /// </summary>
        public static readonly DependencyProperty IsTransparentBackgroundProperty = DependencyProperty.Register(nameof(IsTransparentBackground), typeof(bool), typeof(SpecialKeytop),
            new PropertyMetadata(OnIsTransparentBackgroundChanged));

        /// <summary>
        /// Constructor.
        /// </summary>
        public SpecialKeytop()
        {
            InitializeComponent();
        }

        FrameworkElement IHostedControl.Button { get { return TheKeytop; } }

        /// <summary>
        /// Keytop caption.
        /// </summary>
        public string Keytop
        {
            get { return (string)GetValue(KeytopProperty); }
            set { SetValue(KeytopProperty, value); }
        }

        /// <summary>
        /// Automatically sized keytop caption.
        /// </summary>
        public string SizedKeytop
        {
            get { return (string)GetValue(SizedKeytopProperty); }
            set { SetValue(SizedKeytopProperty, value); }
        }

        /// <summary>
        /// Command to execute for keytop.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Name of action to perform.
        /// </summary>
        internal string ActionName { get; set; }

        string IGazeTarget.SendKeys { get { return null; } }

        /// <summary>
        /// Multiplier applied to time calculation.
        /// </summary>
        public double Multiplier
        {
            get { return (double)GetValue(MultiplierProperty); }
            set { SetValue(MultiplierProperty, value); }
        }

        /// <summary>
        /// Multiplier applied to repetition time calculation.
        /// </summary>
        public double RepeatMultiplier
        {
            get { return (double)GetValue(RepeatMultiplierProperty); }
            set { SetValue(RepeatMultiplierProperty, value); }
        }

        /// <summary>
        /// Vocalisation.
        /// </summary>
        public string Vocal
        {
            get { return (string)GetValue(VocalProperty); }
            set { SetValue(VocalProperty, value); }
        }

        /// <summary>
        /// Is transparent background.
        /// </summary>
        public bool IsTransparentBackground
        {
            get { return (bool)GetValue(IsTransparentBackgroundProperty); }
            set { SetValue(IsTransparentBackgroundProperty, value); }
        }

        static void OnIsTransparentBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var keytop = (SpecialKeytop)d;
                keytop.TheKeytop.Background = Brushes.Transparent;
            }
        }

        /// <summary>
        /// Host for control.
        /// </summary>
        public IKeyboardHost KeyboardHost
        {
            get
            {
                var element = (FrameworkElement)this;
                while (keyboardHostField == null && element != null)
                {
                    keyboardHostField = element as IKeyboardHost;
                    element = element.Parent as FrameworkElement;
                }

                return keyboardHostField;
            }
            set
            {
                keyboardHostField = value;
            }
        }
        IKeyboardHost keyboardHostField;

        void IHostedControl.SetMultiplier(double multiplier, double repeatMultiplier)
        {
            Multiplier = multiplier;
            RepeatMultiplier = repeatMultiplier;
        }

        void Character_Button_Click(object sender, RoutedEventArgs e)
        {
            var host = KeyboardHost;
            if (host != null)
            {
                var action = host.GetAction(ActionName);
                if (action != null)
                {
                    action.Execute(this);
                }
            }

            var command = Command;
            if (command != null && command.CanExecute(this))
            {
                command.Execute(this);
            }
        }
    }
}
