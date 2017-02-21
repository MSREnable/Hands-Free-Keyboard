using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings.Nudgers;
using Microsoft.HandsFree.Settings.Serialization;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Settings for voice.
    /// </summary>
    public abstract class NarrationSettings : NotifyingObject, INarrationSettings
    {
        /// <summary>
        /// No device setting.
        /// </summary>
        public const string None = "None";

        readonly SentenceBehavior _sentenceBehaviorDefault;

        readonly bool _readCompletedWordsDefault;

        readonly bool _readKeyTopsDefault;

        readonly bool _isClickOnDefault;

        readonly bool _playVocalGesturesDefault;

        /// <summary>
        /// Constructor.
        /// </summary>
        public NarrationSettings(SentenceBehavior sentenceBehaviorDefault,
            bool readCompletedWordsDefault,
            bool readKeyTopsDefault,
            bool isClickOnDefault,
            bool playVocalGesturesDefault,
            string deviceDescription)
        {
            _sentenceBehaviorDefault = sentenceBehaviorDefault;
            _readCompletedWordsDefault = readCompletedWordsDefault;
            _readKeyTopsDefault = readKeyTopsDefault;
            _isClickOnDefault = isClickOnDefault;
            _playVocalGesturesDefault = playVocalGesturesDefault;

            _sentenceBehavior = _sentenceBehaviorDefault;
            _readCompletedWords = _readCompletedWordsDefault;
            _readKeyTops = _readKeyTopsDefault;
            _isClickOn = _isClickOnDefault;
            _playVocalGestures = _playVocalGesturesDefault;

            DeviceNudger = new DynamicValueNudger(this, nameof(Device), deviceDescription, "Unknown: {0}");
            SentenceVoicingNudger = new DynamicValueNudger(this, nameof(SentenceVoicing), "Sentence Speaking Voice", "Unknown: {0}");
            WordVoicingNudger = new DynamicValueNudger(this, nameof(WordVoicing), "Word Voice", "Unknown: {0}");
            LetterVoicingNudger = new DynamicValueNudger(this, nameof(LetterVoicing), "Letter Voice", "Unknown: {0}");

            PropertyChanged += OnNarrationSettingsPropertyChanged;
        }

        /// <summary>
        /// Nudger for device selection.
        /// </summary>
        [XmlIgnore]
        public IDynamicValueNudger DeviceNudger { get; }

        /// <summary>
        /// Output device.
        /// </summary>
        public abstract string Device { get; set; }

        /// <summary>
        /// Baked in voicing theme.
        /// </summary>
        [SettingDescription("Built-in Theme")]
        public NarrationTheme NarrationTheme { get { return _voicingTheme; } set { SetProperty(ref _voicingTheme, value); } }
        NarrationTheme _voicingTheme;

        /// <summary>
        /// Nudger to update voice settings.
        /// </summary>
        [XmlIgnore]
        public IDynamicValueNudger SentenceVoicingNudger { get; }

        /// <summary>
        /// Identifier of voice to use.
        /// </summary>
        [SettingDescription("Sentence Speaking Voice", SettingNudgerFactoryBehavior.NameAssociatedMember)]
        public string SentenceVoicing { get { return _sentenceVoice; } set { SetProperty(ref _sentenceVoice, value); } }
        string _sentenceVoice = string.Empty;

        /// <summary>
        /// Should sentence be spoken or just the command word.
        /// </summary>
        [SettingDescription("Say completed sentence")]
        public SentenceBehavior SentenceBehavior { get { return _sentenceBehavior; } set { SetProperty(ref _sentenceBehavior, value); } }
        SentenceBehavior _sentenceBehavior;

        /// <summary>
        /// Behavior of Play button while currently speaking.
        /// </summary>
        [SettingDescription("Concurrent Speech Behavior")]
        public SpeechConcurrencyBehavior SpeechConcurrencyBehavior { get { return _speechConcurrencyBehavior; } set { SetProperty(ref _speechConcurrencyBehavior, value); } }
        SpeechConcurrencyBehavior _speechConcurrencyBehavior = SpeechConcurrencyBehavior.SuppressDuplicates;

        /// <summary>
        /// Number of seconds to elapse before partial sentence recap deemed necessary.
        /// </summary>
        [SettingDescription("Sentence Recap Delay (0 for never)", 0, 30)]
        public int SentenceRecapThreshold { get { return _sentenceRecapThreshold; } set { SetProperty(ref _sentenceRecapThreshold, value); } }
        int _sentenceRecapThreshold;

        /// <summary>
        /// Should completed words be read.
        /// </summary>
        [SettingDescription("Read words as they are completed")]
        public bool ReadCompletedWords { get { return _readCompletedWords; } set { SetProperty(ref _readCompletedWords, value); } }
        bool _readCompletedWords;

        /// <summary>
        /// Nudger to update voice settings.
        /// </summary>
        [XmlIgnore]
        public IDynamicValueNudger WordVoicingNudger { get; private set; }

        /// <summary>
        /// Identifier of voice to use.
        /// </summary>
        [SettingDescription("Word Voice", SettingNudgerFactoryBehavior.NameAssociatedMember)]
        public string WordVoicing { get { return _wordVoice; } set { SetProperty(ref _wordVoice, value); } }
        string _wordVoice = string.Empty;

        /// <summary>
        /// The rate at which words are voiced.
        /// </summary>
        [SettingDescription("Word Voicing Rate", 1, 5)]
        public int WordVoicingRate { get { return _wordVoicingRate; } set { SetProperty(ref _wordVoicingRate, value); } }
        int _wordVoicingRate = 1;

        /// <summary>
        /// Number of ms to wait before starting to fill silence.
        /// </summary>
        [SettingDescription("Delay before filling silence", 0, 10000, 100)]
        public int SilenceFillerDelay { get { return _silenceFillerDelay; } set { SetProperty(ref _silenceFillerDelay, value); } }
        int _silenceFillerDelay;

        /// <summary>
        /// Mechanism for filling silence.
        /// </summary>
        [SettingDescription("Silence Filler")]
        public SilenceFiller SilenceFiller { get { return _silenceFiller; } set { SetProperty(ref _silenceFiller, value); } }
        SilenceFiller _silenceFiller;

        /// <summary>
        /// Percentage volume of silence filler.
        /// </summary>
        [SettingDescription("Silence Filler Volume", 5, 100, 5)]
        public int SilenceFillerVolume { get { return _silenceFillerVolume; } set { SetProperty(ref _silenceFillerVolume, value); } }
        int _silenceFillerVolume = 15;

        /// <summary>
        /// Should individual key tops be read.
        /// </summary>
        [SettingDescription("Read key tops")]
        public bool ReadKeyTops { get { return _readKeyTops; } set { SetProperty(ref _readKeyTops, value); } }
        bool _readKeyTops;

        /// <summary>
        /// Read the top suggestion.
        /// </summary>
        [SettingDescription("Read top suggestions")]
        public bool ReadTopSuggestion { get { return _readTopSuggestion; } set { SetProperty(ref _readTopSuggestion, value); } }
        bool _readTopSuggestion;

        /// <summary>
        /// Should letters be read like a cheerleader?
        /// </summary>
        [SettingDescription("Cheerleader style")]
        public bool IsCheerleaderMode { get { return _isCheerleaderMode; } set { SetProperty(ref _isCheerleaderMode, value); } }
        bool _isCheerleaderMode;

        /// <summary>
        /// Nudger to update voice settings.
        /// </summary>
        [XmlIgnore]
        public IDynamicValueNudger LetterVoicingNudger { get; private set; }

        /// <summary>
        /// Identifier of voice to use.
        /// </summary>
        [SettingDescription("Letter Voice", SettingNudgerFactoryBehavior.NameAssociatedMember)]
        public string LetterVoicing { get { return _letterVoice; } set { SetProperty(ref _letterVoice, value); } }
        string _letterVoice = string.Empty;

        /// <summary>
        /// Make a click sound if no other sound.
        /// </summary>
        [SettingDescription("Click When Keys Pressed")]
        public bool IsClickOn { get { return _isClickOn; } set { SetProperty(ref _isClickOn, value); } }
        bool _isClickOn;

        /// <summary>
        /// Play the vocal gestures like laughing, coughing, ugh, and argh. If false, only a click will be played
        /// </summary>
        [SettingDescription("Play Vocal Sound Effects")]
        public bool PlayVocalGestures { get { return _playVocalGestures; } set { SetProperty(ref _playVocalGestures, value); } }
        bool _playVocalGestures;

        /// <summary>
        /// Should we play sound effects.
        /// </summary>
        [SettingDescription("Play Sound Effects")]
        public bool PlaySoundEffects { get { return _playSoundEffects; } set { SetProperty(ref _playSoundEffects, value); } }
        bool _playSoundEffects;

        void OnNarrationSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NarrationTheme))
            {
                NarrationThemeSettings.SetValues(this);
            }
            else
            {
                NarrationThemeSettings.SetIndicator(this);
            }
        }
    }
}
