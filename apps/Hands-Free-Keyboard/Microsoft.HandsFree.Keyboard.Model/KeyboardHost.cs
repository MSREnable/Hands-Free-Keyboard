using Microsoft.HandsFree.Helpers.Telemetry;
using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Engine;
using Microsoft.HandsFree.Settings;
using Microsoft.HandsFree.SlackClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Microsoft.HandsFree.Keyboard.Model
{
    public sealed class KeyboardHost : NotifyPropertyChangedBase, IKeyboardHost
    {
        readonly IKeyboardApplicationEnvironment _environment;

        // TODO: (JBeavers) redo telemetry
        //int _backspaceCount;
        //int _keyCount;
        //int _timerKeyCount;

        bool _isAutoSpaceNeeded;
        bool _isAutoSpaceAdded;

        readonly IPredictor _predictor;

        TextSlice _predictionSlice = TextSlice.Empty;

        IPrediction _queuedPrediction;
        bool _suggesterRunning;

        readonly INarrator _narrator;

        DateTime _trainingStartTime;

        int _trainingKeyCount;

        NarrationEventArgs _lastNarrationEventArgs;
        readonly List<List<string>> _ambiguousKeys;

        readonly TextBoxEditor _editor;

        static readonly DateTime _arbitraryEpoch = new DateTime(2000, 1, 1, 0, 0, 0);
        readonly DispatcherTimer _minuteTick = new DispatcherTimer();

        KeyboardHost(IKeyboardApplicationEnvironment environment)
        {
            _environment = environment;
            _editor = new TextBoxEditor(_environment.ClipboardProvider);

            SetTrainingPhrase();
            _environment.AppSettings.Keyboard.AttachPropertyChangedAction(nameof(_environment.AppSettings.Keyboard.IsTrainingMode), SetTrainingPhrase);

            _isUpdateAvailable = _environment.IsUpdateAvailable;
            _environment.UpdateAvailable += (s, e) => IsUpdateAvailable = _environment.IsUpdateAvailable;

            var publicTextToAudio = _environment.TextToAudioProviderFactory.Create(AppSettings.Instance.PublicNarration);
            var privateTextToAudio = _environment.TextToAudioProviderFactory.Create(AppSettings.Instance.PrivateNarration);

            ActivityDisplayStatus = _environment.ActivityDisplayProvider.Status;
            _environment.ActivityDisplayProvider.StatusChanged += (s, e) => ActivityDisplayStatus = _environment.ActivityDisplayProvider.Status;

            var publicNarrator = new Narrator(environment.Random, AppSettings.Instance.PublicNarration, _environment.AudioProviderFactory, publicTextToAudio, _environment.ActivityDisplayProvider);
            var privateNarrator = new Narrator(environment.Random, AppSettings.Instance.PrivateNarration, _environment.AudioProviderFactory, privateTextToAudio, NullActivityDisplayProvider.Instance);
            _narrator = new CompoundNarrator(publicNarrator, privateNarrator);

            Speak = new RelayCommand(SpeakAction);
            SendToSlack = new RelayCommand(SendToSlackAction, (o) => AppSettings.Instance.General.SlackHostUri != GeneralSettings.UnsetSlackHostUri);
            Clear = new RelayCommand(ClearAction);
            UpdateApplication = new RelayCommand(UpdateApplicationAction);
            ExitApplication = new RelayCommand(ExitApplicationAction);
            Calibrate = new RelayCommand(CalibrateAction);

            for (var index = 0; index < 7; index++)
            {
                suggestionItems.Add(new SuggestionItem(this, index));
            }
            for (var index = 0; index < 6; index++)
            {
                phraseItems.Add(new PhraseItem(this, index));
            }

            ShiftToggleState = ToggleStates["Shift"];
            ControlToggleState = ToggleStates["Control"];
            AltToggleState = ToggleStates["Alt"];

            ShiftToggleState.CheckChanged += OnShiftChanged;

            _predictor = PredictionEngineFactory.Create(PredictionEnvironment.Instance);
            _predictor.PredictionChanged += OnPredictionChanged;

            _ambiguousKeys = new List<List<string>>();

            _minuteTick.Tick += OnMinuteTick;
            ScheduleNextMinuteTick();

            ShowActiveListening = new RelayCommand((o) => AppSettings.Instance.Prediction.PredictionLayout = PredictionLayout.ActiveListening);
            ShowWordPrediction = new RelayCommand((o) => AppSettings.Instance.Prediction.PredictionLayout = PredictionLayout.WordsAlone);
            ShowPhrasePrediction = new RelayCommand((o) => AppSettings.Instance.Prediction.PredictionLayout = PredictionLayout.PhraseAlone);

            ActiveListeningLaugh = new RelayCommand((o) => SpeakEffect(AudioGesture.Laugh));
            ActiveListeningHmm = new RelayCommand((o) => SpeakEffect(AudioGesture.Hmm));
            ActiveListeningSarcasm = new RelayCommand((o) => SpeakEffect(AudioGesture.Sarcasm));
            ActiveListeningOh = new RelayCommand((o) => SpeakEffect(AudioGesture.Oh));
            ActiveListeningSharpBreath = new RelayCommand((o) => SpeakEffect(AudioGesture.SharpBreath));
            ActiveListeningArgh = new RelayCommand((o) => SpeakEffect(AudioGesture.Argh));
            ActiveListeningCough = new RelayCommand((o) => SpeakEffect(AudioGesture.Cough));
            ActiveListeningUgh = new RelayCommand((o) => SpeakEffect(AudioGesture.Ugh));
        }
        public void SpeakEffect(AudioGesture effect)
        {
            var args = NarrationEventArgs.Create(effect);
            _lastNarrationEventArgs = args;
            _narrator.OnNarrationEvent(args);

            TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId<AudioGesture>(), effect.ToString());
        }

        void OnMinuteTick(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(Time));

            ScheduleNextMinuteTick();
        }

        void ScheduleNextMinuteTick()
        {
            var now = DateTime.Now;
            var epochDelta = now - _arbitraryEpoch;
            var minutesFromEpoch = Math.Floor(epochDelta.TotalMinutes) + 1;
            var nextMinute = _arbitraryEpoch + TimeSpan.FromMinutes(minutesFromEpoch);
            var nextMinuteDelta = nextMinute - now;
            _minuteTick.Interval = nextMinuteDelta;
            _minuteTick.IsEnabled = true;
        }

        /// <summary>
        /// Create a new instance of the host for the environment.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>The new host.</returns>
        public static KeyboardHost Create(IKeyboardApplicationEnvironment environment)
        {
            var host = new KeyboardHost(environment);

            host.SetTextHint(host.TextSlice);

            host.ShiftToggleState.IsChecked = environment.SystemProvider.IsShiftKeyDown;
            host.ControlToggleState.IsChecked = environment.SystemProvider.IsControlKeyDown;
            host.AltToggleState.IsChecked = environment.SystemProvider.IsAltKeyDown;

            return host;
        }

        #region Properties
        public ICommand Speak { get; }

        public ICommand SendToSlack { get; }

        public ICommand Clear { get; }

        public ICommand Calibrate { get; }

        public ICommand UpdateApplication { get; }

        public ICommand ExitApplication { get; }

        public ICommand ShowActiveListening { get; }

        public ICommand ShowWordPrediction { get; }

        public ICommand ShowPhrasePrediction { get; }

        public ICommand ActiveListeningLaugh { get; }
        public ICommand ActiveListeningHmm { get; }
        public ICommand ActiveListeningSarcasm { get; }
        public ICommand ActiveListeningOh { get; }
        public ICommand ActiveListeningSharpBreath { get; }
        public ICommand ActiveListeningArgh { get; }
        public ICommand ActiveListeningCough { get; }
        public ICommand ActiveListeningUgh { get; }

        public ToggleState ShiftToggleState { get; }
        ToggleState ControlToggleState { get; }
        ToggleState AltToggleState { get; }

        public TextSlice TextSlice
        {
            get { return _textSlice; }
            set
            {
                if (SetProperty(ref _textSlice, value))
                {
                    _editor.TextSlice = value;

                    ReflectTextUpdate();

                    var validText = TextSlice.Text.Trim().Length > 0;
                    IsSpeakEnabled = validText;
                }
            }
        }
        TextSlice _textSlice = TextSlice.Empty;

        public string TrainingPhrase
        {
            get { return _trainingPhrase; }
            set { SetProperty(ref _trainingPhrase, value); }
        }
        string _trainingPhrase;

        public string TrainingPhrasePrevious
        {
            get { return _trainingPhrasePrevious; }
            set { SetProperty(ref _trainingPhrasePrevious, value); }
        }
        string _trainingPhrasePrevious = string.Empty;

        public double TrainingScore
        {
            get { return _trainingScore; }
            set { SetProperty(ref _trainingScore, value); }
        }
        double _trainingScore;

        public double TrainingWpm
        {
            get { return _trainingWpm; }
            set { SetProperty(ref _trainingWpm, value); }
        }
        double _trainingWpm;

        public int TrainingKeysUsed
        {
            get { return _trainingKeysUsed; }
            set { SetProperty(ref _trainingKeysUsed, value); }
        }
        int _trainingKeysUsed;

        public Visibility TrainingVisibility
        {
            get { return _trainingVisibility; }
            set { SetProperty(ref _trainingVisibility, value); }
        }
        Visibility _trainingVisibility = Visibility.Collapsed;

        public AppSettings Settings => AppSettings.Instance;

        public ToggleStateCollection ToggleStates { get; } = new ToggleStateCollection();

        public string Time => DateTime.Now.ToShortTimeString();

        public bool IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            private set { SetProperty(ref _isUpdateAvailable, value); }
        }
        bool _isUpdateAvailable;

        public string SpeakKeytop
        {
            get { return _speakKeytop; }
            set { SetProperty(ref _speakKeytop, value); }
        }
        string _speakKeytop = "\xE102";

        public bool IsSpeakEnabled
        {
            get { return _isSpeakEnabled; }
            set { SetProperty(ref _isSpeakEnabled, value); }
        }
        bool _isSpeakEnabled;

        public bool InActiveListeningMode
        {
            get { return _isInActiveListeningMode; }
            set { SetProperty(ref _isInActiveListeningMode, value); }
        }
        bool _isInActiveListeningMode = false;

        public bool IsYesNoEnabled
        {
            get { return _isYesNoEnabled; }
            set { SetProperty(ref _isYesNoEnabled, value); }
        }
        bool _isYesNoEnabled = true;

        public bool IsSentenceEnabled
        {
            get { return _isSentenceEnabled; }
            set { SetProperty(ref _isSentenceEnabled, value); }
        }
        bool _isSentenceEnabled = true;

        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set { SetProperty(ref _isAllSelected, value); }
        }
        bool _isAllSelected = true;

        public string ApplicationVersion => _applicationVersion;
        static readonly string _applicationVersion = GetApplicationVersion();
        // TODO: The following code really belongs in some Hands Free branding assembly that contains code common to all Enable products.
        static string GetApplicationVersion()
        {
            string version;

            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var number = assembly.GetName().Version;
                var branch = ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)))?.InformationalVersion;
                version = $"{branch}-{number}";
            }
            else
            {
                version = "UnitTest?";
            }

            return version;
        }

        public bool IsInPrivate
        {
            get { return _isInPrivate; }
            set
            {
                if (SetProperty(ref _isInPrivate, value))
                {
                    NotifyPropertyChanged(nameof(IsInPrivateText));
                }
            }
        }
        bool _isInPrivate;

        public string IsInPrivateText { get { return IsInPrivate ? "IN PRIVATE" : "LOGGING"; } }

        /// <summary>
        /// Is the user interface paused?
        /// </summary>
        public bool IsPaused
        {
            get { return _isPaused; }
            set { SetProperty(ref _isPaused, value); }
        }
        bool _isPaused;

        public double WPM { get { return _wpm; } set { SetProperty(ref _wpm, value); } }
        double _wpm;

        public double ErrorRate { get { return _errorRate; } set { SetProperty(ref _errorRate, value); } }
        double _errorRate;

        public Point SignalMean { get { return _signalMean; } set { SetProperty(ref _signalMean, value); } }
        Point _signalMean;

        public Point SignalStandardDeviation { get { return _signalStandardDeviation; } set { SetProperty(ref _signalStandardDeviation, value); } }
        Point _signalStandardDeviation;

        public List<SuggestionItem> SuggestionItems { get { return suggestionItems; } }
        readonly List<SuggestionItem> suggestionItems = new List<SuggestionItem>();

        public List<PhraseItem> PhraseItems { get { return phraseItems; } }
        readonly List<PhraseItem> phraseItems = new List<PhraseItem>();

        public string ActivityDisplayStatus { get { return _activityDisplayStatus; } set { SetProperty(ref _activityDisplayStatus, value); } }
        string _activityDisplayStatus;

        #endregion // Properties

        #region Actions
        public ICommand GetAction(string name)
        {
            var command = HandlerToCommand.Create(this, name);
            return command;
        }

        void SpeakAction(object o)
        {
            RaiseNarrationEventWithPossibleWordCompletion(o, NarrationEventType.Utter);

            if (_environment.AppSettings.Keyboard.IsTrainingMode)
            {
                if (!IsAllSelected)
                {
                    TrainingPhrasePrevious = TrainingPhrase;

                    TrainingScore = LevenshtienDistanceScore(TextSlice.Text, TrainingPhrase);

                    TrainingKeysUsed = _trainingKeyCount;

                    var duration = DateTime.UtcNow - _trainingStartTime;
                    TrainingWpm = TextSlice.Text.Length / 5.0 / duration.TotalMinutes;
                    TrainingVisibility = Visibility.Visible;
                    SetTrainingPhrase();
                }
            }
            else
            {
                TrainingVisibility = Visibility.Collapsed;
            }

            TextSlice = _editor.SelectAll();
            _isAutoSpaceNeeded = false;

            _predictor.RecordHistory(TextSlice.Text, IsInPrivate);

            TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(EventId.PhrasesSpoken), "1");
            var words = TextSlice.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            TelemetryMessage.Telemetry.TraceEvent(TraceEventType.Information, IdMapper.GetId(EventId.WordsSpoken), words.ToString());
        }

        void ListenAction(object o)
        {
            InActiveListeningMode = true;
        }

        void SendToSlackAction(object o)
        {
            RaiseNarrationEvent(o, NarrationEventType.Simple);

            if (_environment.AskYesNo("Do you want to send feedback to Slack?"))
            {
                var target = SlackClientFactory.Create();
                target.Send(TextSlice.Text);
                _predictor.RecordHistory(TextSlice.Text, false);
            }
        }

        void ClearAction(object o)
        {
            RaiseNarrationEvent(o, NarrationEventType.Reset);

            TextSlice = _editor.Clear();
        }

        void CalibrateAction(object o)
        {
            RaiseNarrationEvent(o, NarrationEventType.Simple);

            _environment.GazeProvider.LaunchRecalibration();
        }

        void UpdateApplicationAction(object o)
        {
            _environment.RestartWithUpdate();
        }

        void ExitApplicationAction(object o)
        {
            RaiseNarrationEvent(o, NarrationEventType.Simple);

            // TODO: Should wait for narration to complete rather than sleeping here.
            Thread.Sleep(500);

            _environment.ExitApplication();
        }
        #endregion // Actions

        string GetCurrentWord()
        {
            var text = TextSlice.Text;
            var end = TextSlice.Start;
            var wordLength = text.ReverseWordLength(end);
            return wordLength == 0 ? null : text.Substring(end - wordLength, wordLength);
        }

        static bool IsPunctuation(string key)
        {
            return key.Length == 1 && (char.IsPunctuation(key[0]));
        }

        #region NarrationEvent
        void RaiseNarrationEvent(object key, NarrationEventType eventType, string completedWord)
        {
            if (IsAllSelected)
            {
                _trainingStartTime = DateTime.UtcNow;
                _trainingKeyCount = 1;
            }
            else
            {
                _trainingKeyCount++;
            }

            var args = NarrationEventArgs.Create(key, eventType, TextSlice.Text, TextSlice.Start, IsAllSelected, completedWord);
            _lastNarrationEventArgs = args;
            _narrator.OnNarrationEvent(args);
        }

        void RaiseNarrationEvent(object key, NarrationEventType eventType)
        {
            RaiseNarrationEvent(key, eventType, null);
        }

        void RaiseNarrationEventWithPossibleWordCompletion(object key, NarrationEventType eventType)
        {
            string word;

            if (_isAutoSpaceNeeded)
            {
                word = null;
            }
            else
            {
                word = GetCurrentWord();
            }

            if (word != null)
            {
                RaiseNarrationEvent(key, eventType, word);
            }
            else
            {
                RaiseNarrationEvent(key, eventType);
            }
        }
        #endregion

        void IKeyboardHost.PlaySimpleKeyFeedback(string vocal)
        {
            RaiseNarrationEvent(vocal, NarrationEventType.Simple);
        }

        void IKeyboardHost.ShowException(string context, Exception ex)
        {
            var message = $"{context}: {ex.Message}";
            TextSlice = new TextSlice(message, 0, message.Length, true);
        }

        static string GetVocalization(object o)
        {
            var special = o as SpecialKeytop;
            var vocal = special != null ? (special.Vocal ?? special.Keytop) : o as string;
            return vocal ?? "wibble";
        }

        #region Training
        void SetTrainingPhrase()
        {
            string sentence;

            if (_environment.AppSettings.Keyboard.IsTrainingMode)
            {
                var choice = _environment.Random.Next(TestSentences.Instance.Count);
                sentence = TestSentences.Instance[choice];
            }
            else
            {
                sentence = string.Empty;
            }

            TrainingPhrase = sentence;
        }

        static double LevenshtienDistanceScore(String lhs, String rhs)
        {
            var lhsLength = lhs.Length;
            int rhsLength = rhs.Length;

            var answer = new int[lhsLength + 1];

            for (var rowIndex = 0; rowIndex <= lhsLength; rowIndex++)
            {
                answer[rowIndex] = rowIndex;
            }

            var workspace = new int[lhsLength + 1];

            for (var columnIndex = 0; columnIndex < rhsLength; columnIndex++)
            {
                var temp = workspace;
                workspace = answer;
                answer = temp;

                answer[0] = columnIndex + 1;

                var columnChar = rhs[columnIndex];

                for (var rowIndex = 0; rowIndex < lhsLength; rowIndex++)
                {
                    var rowChar = lhs[rowIndex];

                    var a = workspace[rowIndex + 1] + 1;
                    var b = answer[rowIndex] + 1;
                    var c = workspace[rowIndex] + (rowChar == columnChar ? 0 : 1);

                    answer[rowIndex + 1] = Math.Min(Math.Min(a, b), c);
                }
            }

            return 1 - (double)answer[lhsLength] / Math.Max(lhsLength, rhsLength);
        }
        #endregion

        void SpeakCommand(object o, EventArgs e)
        {
            SpeakFixedText((o as SpecialKeytop).Keytop);
        }

        void SendAmbiguousKeys(object o, EventArgs e)
        {
            char[] separator = new char[] { ' ' };
            var key = o as SpecialKeytop;
            var chars = new List<string>(key.Keytop.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            _ambiguousKeys.Add(chars);
            SendAlphanumericKeyPress(chars[0]);
        }

        #region Suggestion
        void InsertSuggestion(IPredictionSuggestion suggestion, string insert)
        {
            if ((_isAutoSpaceNeeded) && (suggestion.CompleteWord))
            {
                _isAutoSpaceNeeded = false;

                insert = " " + insert;
            }
            _isAutoSpaceAdded = false;

            Debug.Assert(!_isAutoSpaceNeeded);
            Debug.Assert(!_isAutoSpaceAdded);

            if (!suggestion.CompleteWord)
            {
                insert = insert.Substring(0, insert.Length - 1);
            }

            switch (Settings.Prediction.PredictionSpacingBehavior)
            {
                case PredictionSpacingBehavior.Always:
                    insert += " ";
                    break;

                case PredictionSpacingBehavior.Never:
                    break;

                case PredictionSpacingBehavior.AddAndRemoveIfUnwanted:
                    insert += " ";
                    _isAutoSpaceAdded = true;
                    break;

                case PredictionSpacingBehavior.AddIfNeeded:
                default:
                    Debug.Assert(Settings.Prediction.PredictionSpacingBehavior == PredictionSpacingBehavior.AddIfNeeded);

                    _isAutoSpaceNeeded = suggestion.CompleteWord;
                    break;
            }

            var originalText = TextSlice.Text;
            var replacementText = originalText.Substring(0, suggestion.ReplacementStart) +
                insert +
                originalText.Substring(suggestion.ReplacementStart + suggestion.ReplacementLength);

            var slice = new TextSlice(replacementText, suggestion.ReplacementStart + insert.Length);
            if (slice != TextSlice)
            {
                // Normally selecting a prediction will change the text.
                _editor.TextSlice = slice;
                TextSlice = slice;
            }
            else
            {
                // Sometimes the prediction is just what we typed, but we still want to fire off the prediction engine
                // which usually runs as a side-effect of the text changing. (This is probably the same as detecting that
                // _isAutoSpaceNeeded changed as a side-effect of this method.)
                SetTextHint(slice);
            }

            // Release the Shift key if it had been pressed.
            ShiftToggleState.IsChecked = false;

            _ambiguousKeys.Clear();
            if (!suggestion.CompleteWord)
            {
                var entry = new List<string>();
                entry.Add(insert);
                _ambiguousKeys.Add(entry);
            }

            RaiseNarrationEvent(insert, NarrationEventType.AcceptSuggestion, insert);
        }

        internal void MakeSuggestion(SuggestionItem item)
        {
            var insert = item.Keytop;

            var suggestion = item.Suggestion;
            InsertSuggestion(suggestion, insert);
        }

        internal void AcceptPhrase(PhraseItem item)
        {
            var insert = SuggestionItems[0].Keytop;
            for (var i = 0; i <= item.Index; i++)
            {
                insert += " " + PhraseItems[i].Keytop;
            }

            var suggestion = SuggestionItems[0].Suggestion;
            InsertSuggestion(suggestion, insert);
        }

        void ReflectTextUpdate()
        {
            SetTextHint(TextSlice);
        }

        void SuggesterThread(object state)
        {
            Thread.Sleep(100);
            var prediction = _environment.Dispatcher.Invoke(() => { var r = _queuedPrediction; _queuedPrediction = null; return r; });
            do
            {
                var seedLength = _predictionSlice.Text.ReverseWordLength(_predictionSlice.Start);

                var wordSuggestions = prediction.GetSuggestions(SuggestionType.Word);
                var wordSuggestionsUncached = new List<IPredictionSuggestion>(wordSuggestions);

                var phraseSuggestions = prediction.GetSuggestions(SuggestionType.Phrase);
                var phraseSuggestionsUncached = new List<IPredictionSuggestion>(phraseSuggestions);

                prediction = _environment.Dispatcher.Invoke(() => DisplaySuggestions(seedLength, wordSuggestionsUncached, phraseSuggestionsUncached));
            }
            while (prediction != null);
        }

        IPrediction DisplaySuggestions(int seedLength, IEnumerable<IPredictionSuggestion> wordSuggestions, IEnumerable<IPredictionSuggestion> phraseSuggestions)
        {
            if (_queuedPrediction == null)
            {
                var isShifted = ShiftToggleState.IsChecked;

                var suggestions = new List<string>();

                var itemPosition = 0;
                foreach (var suggestion in wordSuggestions)
                {
                    if (itemPosition < SuggestionItems.Count)
                    {
                        var item = SuggestionItems[itemPosition];
                        var shiftedText = ShiftText(isShifted, suggestion.Text);
                        item.Keytop = shiftedText;
                        item.Visibility = Visibility.Visible;
                        item.IsEnabled = true;
                        item.Suggestion = suggestion;

                        suggestions.Add(shiftedText);
                    }

                    itemPosition++;
                }

                while (itemPosition < SuggestionItems.Count)
                {
                    var item = SuggestionItems[itemPosition];
                    itemPosition++;

                    item.Visibility = Visibility.Collapsed;
                    item.Suggestion = null;
                }

                SuggestionsHelper.SetSuggestions(suggestions);

                if (suggestions.Count != 0 && _lastNarrationEventArgs != null &&
                    (seedLength != 0 ||
                    _environment.AppSettings.Prediction.PredictionNovelty == PredictionNovelty.FromScratch))
                {
                    var args = NarrationEventArgs.Create(_lastNarrationEventArgs, suggestions[0]);
                    _narrator.OnNarrationEvent(args);
                }

                itemPosition = 0;
                foreach (var phrase in phraseSuggestions)
                {
                    if (itemPosition < PhraseItems.Count)
                    {
                        var item = PhraseItems[itemPosition];
                        item.Keytop = phrase.Text;
                        item.Visibility = Visibility.Visible;
                        item.IsEnabled = true;
                        item.Suggestion = phrase;
                    }

                    itemPosition++;
                }

                while (itemPosition < PhraseItems.Count)
                {
                    var item = PhraseItems[itemPosition];
                    itemPosition++;

                    item.Visibility = Visibility.Collapsed;
                    item.Suggestion = null;
                }
            }

            var prediction = _queuedPrediction;
            _queuedPrediction = null;

            _suggesterRunning = prediction != null;
            return prediction;
        }

        void SetTextHint(TextSlice slice)
        {
            _predictionSlice = slice;

            if (slice.Text.Length == 0)
            {
                _ambiguousKeys.Clear();
            }
            var hints = _ambiguousKeys.Count > 0 ? _ambiguousKeys : null;
            _queuedPrediction = _predictor.CreatePrediction(slice.Text, slice.Start, slice.Length, _isAutoSpaceNeeded, hints);

            if (AppSettings.Instance.Prediction.PredictCharacters)
            {
                var characterSuggestions = _queuedPrediction.GetSuggestions(SuggestionType.Character);

                _environment.GazeProvider?.SetCharacterSuggestions(characterSuggestions);
            }

            foreach (var item in SuggestionItems)
            {
                item.IsEnabled = false;
            }

            foreach (var item in PhraseItems)
            {
                item.IsEnabled = false;
            }

            if (!_suggesterRunning)
            {
                _suggesterRunning = true;
                ThreadPool.QueueUserWorkItem(SuggesterThread);
            }
        }

        void OnPredictionChanged(object sender, EventArgs e)
        {
            _environment.Dispatcher.Invoke(() =>
            {
                SetTextHint(_predictionSlice);
            });
        }
        #endregion

        #region Character State
        static string ShiftText(bool isShifted, string original)
        {
            string shifted;

            if (isShifted && !string.IsNullOrWhiteSpace(original))
            {
                shifted = char.ToUpperInvariant(original[0]) + original.Substring(1);
            }
            else
            {
                shifted = original;
            }

            return shifted;
        }

        void OnShiftChanged(object sender, EventArgs e)
        {
            var isShifted = ShiftToggleState.IsChecked;

            foreach (var item in SuggestionItems)
            {
                if (item.Suggestion != null)
                {
                    item.Keytop = ShiftText(isShifted, item.Suggestion.Text);
                }
            }
        }

        string CharacterState()
        {
            var state = ControlToggleState.IsChecked ? "^" : "";
            state += AltToggleState.IsChecked ? "%" : "";

            ShiftToggleState.IsChecked = false;
            ControlToggleState.IsChecked = false;
            AltToggleState.IsChecked = false;

            return state;
        }
        #endregion

        public void SendAlphanumericKeyPress(string key, string vocal)
        {
            var word = GetCurrentWord();
            if (word != null && !_isAutoSpaceNeeded)
            {
                if (key == " ")
                {
                    RaiseNarrationEvent(vocal, NarrationEventType.WordCompletion, word);
                }
                else if (IsPunctuation(key))
                {
                    RaiseNarrationEvent(vocal, NarrationEventType.SimpleTyping, word);
                }
                else
                {
                    RaiseNarrationEvent(vocal, NarrationEventType.SimpleTyping);
                }
            }
            else
            {
                RaiseNarrationEvent(vocal, NarrationEventType.SimpleTyping);
            }

            SendAlphanumericKeyPress(key);
        }

        internal static string AutoEscape(string key)
        {
            string keys;

            switch (key)
            {
                case "(":
                case ")":
                case "{":
                case "}":
                case "^":
                case "%":
                case "+":
                    keys = "{" + key + "}";
                    break;

                default:
                    keys = key;
                    break;
            }

            return keys;
        }

        void SendAlphanumericKeyPress(string key)
        {
            string spacePrefix;
            if (key.Length == 1)
            {
                if (char.IsLetterOrDigit(key[0]))
                {
                    // We have an alphanumeric key.

                    spacePrefix = _isAutoSpaceNeeded ? " " : string.Empty;
                }
                else
                {
                    // We don't have an alphnumeric key.

                    spacePrefix = _isAutoSpaceAdded ? "{BACKSPACE}" : string.Empty;
                }
            }
            else
            {
                spacePrefix = string.Empty;
            }

            // Now no longer in autospace modes.
            _isAutoSpaceNeeded = false;
            _isAutoSpaceAdded = false;

            var keys = AutoEscape(key);
            var statePrefix = CharacterState();
            var sendKeys = spacePrefix + statePrefix + keys;

            // Handle punctuation cases.
            if (key.Length == 1 &&
                char.IsPunctuation(key[0]))
            {
                switch (Settings.Keyboard.PunctuationSpacing)
                {
                    case PredictionSpacingBehavior.Always:
                        sendKeys += ' ';
                        break;

                    case PredictionSpacingBehavior.AddAndRemoveIfUnwanted:
                        sendKeys += ' ';
                        _isAutoSpaceAdded = true;
                        break;

                    case PredictionSpacingBehavior.AddIfNeeded:
                        _isAutoSpaceNeeded = true;
                        break;

                    default:
                    case PredictionSpacingBehavior.Never:
                        Debug.Assert(Settings.Keyboard.PunctuationSpacing == PredictionSpacingBehavior.Never);
                        break;
                }
            }

            TextSlice = _editor.Interpret(sendKeys);

            if (keys == "{BACKSPACE}")
            {
                if (_ambiguousKeys.Count > 0)
                {
                    _ambiguousKeys.RemoveAt(_ambiguousKeys.Count - 1);
                }
                // TODO: (JBeavers) redo telemetry
                // _backspaceCount++;
            }

            // TODO: (JBeavers) redo telemetry
            // _timerKeyCount++;
            // _keyCount++;
        }

        public void SpeakFixedText(string text)
        {
            RaiseNarrationEvent(text, NarrationEventType.FixedUtter);
        }
    }
}

