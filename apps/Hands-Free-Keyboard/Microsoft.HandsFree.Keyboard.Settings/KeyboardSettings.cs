using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings.Nudgers;
using Microsoft.HandsFree.Settings.Serialization;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Keyboard settings.
    /// </summary>
    public class KeyboardSettings : NotifyingObject
    {
        /// <summary>
        /// The keyboard layout.
        /// </summary>
        [SettingDescription("Keyboard Layout")]
        public KeyboardLayoutName KeyboardLayout
        {
            get { return _keyboardLayout; }
            set { SetProperty(ref _keyboardLayout, value); }
        }
        KeyboardLayoutName _keyboardLayout = KeyboardLayoutName.Default;

        /// <summary>
        /// Screen scaling, overrides the scale set in Windows to provide consistent UI scaling.
        /// </summary>
        [SettingDescription("Screen Scaling", 0.25, 2.0, 0.25)]
        public double ScreenScaling
        {
            get { return _screenScaling; }
            set { SetProperty(ref _screenScaling, value); }
        }
        double _screenScaling = 1.5;

        /// <summary>
        /// The scale factor to apply to the keyboard rendering.
        /// </summary>
        [SettingDescription("Keyboard Scale", SettingNudgerFactoryBehavior.NameAssociatedMember)]
        public double KeyboardScale
        {
            get { return _keyboardScale; }
            set { SetProperty(ref _keyboardScale, value); }
        }
        double _keyboardScale = 1;

        /// <summary>
        /// The keyboard scale nudger.
        /// </summary>
        [XmlIgnore]
        public IValueNudger KeyboardScaleNudger { get; private set; }

        /// <summary>
        /// The activation delay multipler to use for the top ranked suggestion.
        /// </summary>
        [SettingDescription("First Suggestion Gaze Delay Multiplier", 0.0, 5.0, 0.1)]
                public double FirstSuggestionDelayMultiplier
        {
            get { return _firstSuggestionDelayMultiplier; }
            set { SetProperty(ref _firstSuggestionDelayMultiplier, value); }
        }
        double _firstSuggestionDelayMultiplier = 1;

        /// <summary>
        /// The activation delay multiplier to use for non-top ranked suggestions.
        /// </summary>
        [SettingDescription("Suggestion Gaze Delay Multiplier", 0.0, 5.0, 0.1)]
        public double SuggestionDelayMultiplier
        {
            get { return _suggestionDelayMultiplier; }
            set { SetProperty(ref _suggestionDelayMultiplier, value); }
        }
        double _suggestionDelayMultiplier = 2;

        /// <summary>
        /// The minimum activation delay to use.
        /// </summary>
        public const int MinGazeClickDelay = 150 + (int)Mouse.GazeMouse.DefaultMouseDownDelay;

        private const int DefaultGazeClickDelay = 300 + (int)Mouse.GazeMouse.DefaultMouseDownDelay;
        /// <summary>
        /// The maximum normal activation delay to use.
        /// </summary>
        [SettingDescription("Maximum Click Delay (default 550ms)", MinGazeClickDelay, 1500, 5)]
        public int GazeClickDelay
        {
            get { return _gazeClickDelay; }
            set { SetProperty(ref _gazeClickDelay, value); }
        }

        private int _gazeClickDelay = DefaultGazeClickDelay;

        /// <summary>
        /// The display theme.
        /// </summary>
        [SettingDescription("Display Theme")]
        public DisplayTheme DisplayTheme { get { return _displayTheme; } set { SetProperty(ref _displayTheme, value); } }
        DisplayTheme _displayTheme = DisplayTheme.Default;

        /// <summary>
        /// Are we to display a target sentence and measure the distance of utterance from it when played?
        /// </summary>
        [SettingDescription("Display training sentences")]
        public bool IsTrainingMode { get { return _isTrainingMode; } set { SetProperty(ref _isTrainingMode, value); } }
        bool _isTrainingMode;

        /// <summary>
        /// Has the EULA been seen and accepted.
        /// </summary>
        [SettingDescription("Accept EULA")]
        public bool IsEulaAccepted { get { return _isEulaAccepted; } set { SetProperty(ref _isEulaAccepted, value); } }
        bool _isEulaAccepted;

        /// <summary>
        /// Constructor.
        /// </summary>
        public KeyboardSettings()
        {
            KeyboardScaleNudger = new DoubleValueNudger(this, nameof(KeyboardScale), "Keyboard Scale", 0.1, 0, 1);
        }
    }

}
