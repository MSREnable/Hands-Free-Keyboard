using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Interaction logic for AlphanumericKeytop.xaml
    /// </summary>
    public sealed partial class ToggleKeytop : UserControl, IHostedControl
    {
        /// <summary>
        /// Keytop caption property.
        /// </summary>
        public static readonly DependencyProperty KeytopProperty = DependencyProperty.Register(nameof(Keytop), typeof(string), typeof(ToggleKeytop));

        /// <summary>
        /// State toggled by key property.
        /// </summary>
        public static readonly DependencyProperty StateNameProperty = DependencyProperty.Register(nameof(StateName), typeof(string), typeof(ToggleKeytop));

        /// <summary>
        /// State set vocalisation property.
        /// </summary>
        public static readonly DependencyProperty SetVocalProperty = DependencyProperty.Register(nameof(SetVocal), typeof(string), typeof(ToggleKeytop));

        /// <summary>
        /// State unset vocalisation property.
        /// </summary>
        public static readonly DependencyProperty UnsetVocalProperty = DependencyProperty.Register(nameof(UnsetVocal), typeof(string), typeof(ToggleKeytop));

        ToggleState _state;

        bool _silentChange;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ToggleKeytop()
        {
            InitializeComponent();

            Loaded += (s, e) =>
                {
                    var host = KeyboardHost;
                    if (host != null)
                    {
                        _state = host.ToggleStates[StateName];

                        _silentChange = true;
                        TheKeytop.IsChecked = _state.IsChecked;
                        _silentChange = false;

                        _state.CheckChanged += OnToggleCheckChanged;
                    }
                };

            Unloaded += (s, e) =>
                {
                    if (_state != null)
                    {
                        _state.CheckChanged -= OnToggleCheckChanged;
                    }
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
        /// Set vocalisation.
        /// </summary>
        public string SetVocal
        {
            get { return (string)GetValue(SetVocalProperty); }
            set { SetValue(SetVocalProperty, value); }
        }

        /// <summary>
        /// Unset vocalisation.
        /// </summary>
        public string UnsetVocal
        {
            get { return (string)GetValue(UnsetVocalProperty); }
            set { SetValue(UnsetVocalProperty, value); }
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
            _silentChange = true;
            TheKeytop.IsChecked = _state.IsChecked;
            _silentChange = false;
        }

        void OnChecked(object sender, RoutedEventArgs e)
        {
            _state.IsChecked = TheKeytop.IsChecked.Value;

            if(!_silentChange)
            {
                KeyboardHost.PlaySimpleKeyFeedback(_state.IsChecked ? (SetVocal ?? StateName) : (UnsetVocal ?? "Release " + StateName));
            }
        }
    }
}
