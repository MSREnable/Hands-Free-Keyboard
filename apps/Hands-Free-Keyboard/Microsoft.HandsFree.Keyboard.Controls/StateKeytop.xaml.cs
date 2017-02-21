using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Interaction logic for AlphanumericKeytop.xaml
    /// </summary>
    public sealed partial class StateKeytop : UserControl, IHostedControl
    {
        /// <summary>
        /// Keytop caption property.
        /// </summary>
        public static readonly DependencyProperty KeytopProperty = DependencyProperty.Register(nameof(Keytop), typeof(string), typeof(StateKeytop));

        /// <summary>
        /// State toggled by key property.
        /// </summary>
        public static readonly DependencyProperty StateNameProperty = DependencyProperty.Register(nameof(StateName), typeof(string), typeof(StateKeytop));

        /// <summary>
        /// Vocal value property.
        /// </summary>
        public static readonly DependencyProperty VocalProperty = DependencyProperty.Register(nameof(Vocal), typeof(string), typeof(StateKeytop));

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateKeytop()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                };

            Unloaded += (s, e) =>
                {
                };
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
        /// Name of state toggle bound to.
        /// </summary>
        public string StateName
        {
            get { return (string)GetValue(StateNameProperty); }
            set { SetValue(StateNameProperty, value); }
        }

        /// <summary>
        /// Vocal version of state.
        /// </summary>
        public string Vocal
        {
            get { return (string)GetValue(VocalProperty); }
            set { SetValue(VocalProperty, value); }
        }

        string IGazeTarget.SendKeys { get { return null; } }

        /// <summary>
        /// Multiplier applied to time calculation.
        /// </summary>
        public double Multiplier { get; private set; }

        /// <summary>
        /// Multiplier applied to repetition time calculation.
        /// </summary>
        public double RepeatMultiplier { get; private set; }

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

        void OnToggleCheckChanged(object sender, EventArgs e)
        {
        }

        void OnChecked(object sender, RoutedEventArgs e)
        {
            KeyboardHost.PlaySimpleKeyFeedback(Vocal ?? StateName);
        }
    }
}
