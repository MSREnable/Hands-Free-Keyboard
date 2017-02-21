using Microsoft.HandsFree.Settings;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Interaction logic for AlphanumericKeytop.xaml
    /// </summary>
    public partial class AlphanumericKeytop : UserControl, IHostedControl
    {
        /// <summary>
        /// Keytop caption property.
        /// </summary>
        public static readonly DependencyProperty KeytopProperty = DependencyProperty.Register(nameof(Keytop), typeof(string), typeof(AlphanumericKeytop));

        /// <summary>
        /// Keytop caption when shifted property.
        /// </summary>
        public static readonly DependencyProperty ShiftKeytopProperty = DependencyProperty.Register(nameof(ShiftKeytop), typeof(string), typeof(AlphanumericKeytop));

        static readonly DependencyPropertyKey EffectiveKeytopPropertyKey = DependencyProperty.RegisterReadOnly(nameof(EffectiveKeytop), typeof(string), typeof(AlphanumericKeytop), new PropertyMetadata());

        /// <summary>
        /// The effective current keytop property.
        /// </summary>
        public static readonly DependencyProperty EffectiveKeytopProperty = EffectiveKeytopPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the System.Windows.Controls.TextBlock.TextDecorations dependency property.
        /// </summary>
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(nameof(TextDecorations), typeof(TextDecorationCollection), typeof(AlphanumericKeytop));

        /// <summary>
        /// The last clicked alphanumeric, the one that will be displaying the suggestion hint.
        /// </summary>
        static AlphanumericKeytop _lastClicked;

        /// <summary>
        /// The timer controlling the lifetime of the suggestion hint.
        /// </summary>
        static readonly DispatcherTimer HideTimer;

        /// <summary>
        /// The shift state, used to select between lowercase and uppercase.
        /// </summary>
        ToggleState _state;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static AlphanumericKeytop()
        {
            // Initialise timer for hiding hints.
            HideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            HideTimer.Tick += OnHideSuggestion;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AlphanumericKeytop()
        {
            InitializeComponent();

            SuggestionsHelper.SuggestionsChanged += OnTopSuggestionChanged;

            Loaded += (s, e) =>
                {
                    var host = KeyboardHost;
                    if (host != null)
                    {
                        _state = KeyboardHost.ToggleStates["Shift"];
                        _state.CheckChanged += SwitchCase;
                    }

                    SwitchCase(null, EventArgs.Empty);
                };
        }

        FrameworkElement IHostedControl.Button { get { return TheKeytop; } }

        /// <summary>
        /// The text decorations.
        /// </summary>
        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        /// <summary>
        /// Keytop caption.
        /// </summary>
        public string Keytop
        {
            get { return (string)GetValue(KeytopProperty); }
            set { SetValue(KeytopProperty, value); }
        }

        /// <summary>
        /// Value to send if not shifted.
        /// </summary>
        internal string SendValue { get; set; }

        /// <summary>
        /// Value to send if shifted.
        /// </summary>
        internal string ShiftSendValue { get; set; }

        internal string Vocal { get; set; }

        internal string ShiftVocal { get; set; }

        internal bool ShowHints { get; set; } = true;

        /// <summary>
        /// Keytop caption when shifted.
        /// </summary>
        public string ShiftKeytop
        {
            get { return (string)GetValue(ShiftKeytopProperty); }
            set { SetValue(ShiftKeytopProperty, value); }
        }

        /// <summary>
        /// The effective keytop being displayed.
        /// </summary>
        public string EffectiveKeytop
        {
            get { return (string)GetValue(EffectiveKeytopProperty); }
            private set { SetValue(EffectiveKeytopPropertyKey, value); }
        }

        string GetEffectiveKeytop()
        {
            string value;

            if (ShiftKeytop != null)
            {
                if (_state.IsChecked)
                {
                    value = ShiftKeytop;
                }
                else
                {
                    value = Keytop;
                }
            }
            else if (Keytop != null && Keytop.Length == 1)
            {
                if (_state.IsChecked)
                {
                    value = Keytop.ToUpperInvariant();
                }
                else
                {
                    value = Keytop.ToLowerInvariant();
                }
            }
            else
            {
                value = Keytop;
            }

            return value;
        }

        /// <summary>
        /// The output keystrokes.
        /// </summary>
        public string SendKeys
        {
            get
            {
                string value;

                if (SendValue == null)
                {
                    value = EffectiveKeytop;
                }
                else
                {
                    value = _state.IsChecked ? (ShiftSendValue ?? SendValue) : SendValue;
                }

                return value;
            }
        }

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
                while (_keyboardHost == null && element != null)
                {
                    _keyboardHost = element as IKeyboardHost;
                    element = element.Parent as FrameworkElement;
                }

                return _keyboardHost;
            }
            set
            {
                _keyboardHost = value;
            }
        }
        IKeyboardHost _keyboardHost;

        void IHostedControl.SetMultiplier(double multiplier, double repeatMultiplier)
        {
            Multiplier = multiplier;
            RepeatMultiplier = repeatMultiplier;
        }

        void SwitchCase(object sender, EventArgs e)
        {
            EffectiveKeytop = GetEffectiveKeytop();
        }

        void Character_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_lastClicked != null)
            {
                _lastClicked.PrimarySuggestion.Text = string.Empty;
            }
            _lastClicked = this;
            HideTimer.Stop();
            var interval = AppSettings.Instance.Prediction.KeyTopHintInterval;
            if (interval != 0)
            {
                HideTimer.Interval = TimeSpan.FromMilliseconds(interval);
                HideTimer.Start();
            }

            var vocal = (_state.IsChecked ? ShiftVocal ?? Vocal : Vocal) ?? Keytop;

            KeyboardHost.SendAlphanumericKeyPress(SendKeys, vocal);
        }

        static void OnHideSuggestion(object sender, EventArgs e)
        {
            HideTimer.Stop();

            if (_lastClicked != null)
            {
                _lastClicked.PrimarySuggestion.Text = string.Empty;
                _lastClicked = null;
            }
        }

        void OnTopSuggestionChanged(object sender, SuggestionsEventArgs e)
        {
            if (_lastClicked == this && ShowHints)
            {
                var settings = AppSettings.Instance;
                var suggestions = settings.Prediction.KeyTopHints;
                PrimarySuggestion.Text = 1 <= suggestions && 1 <= e.Suggestions.Count ? e.Suggestions[0] : string.Empty;
            }
        }

    }
}
