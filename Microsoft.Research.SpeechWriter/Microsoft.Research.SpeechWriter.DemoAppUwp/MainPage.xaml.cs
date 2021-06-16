using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IApplicationHost
    {
        public static DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(ApplicationModel), typeof(MainPage),
            new PropertyMetadata(null, OnModelChanged));
        public static DependencyProperty MoveToCenterXProperty = DependencyProperty.Register(nameof(MoveToCenterX), typeof(double), typeof(MainPage),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToCenterYProperty = DependencyProperty.Register(nameof(MoveToCenterY), typeof(double), typeof(MainPage),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToXProperty = DependencyProperty.Register(nameof(MoveToX), typeof(double), typeof(MainPage),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToYProperty = DependencyProperty.Register(nameof(MoveToY), typeof(double), typeof(MainPage),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToWidthProperty = DependencyProperty.Register(nameof(MoveToWidth), typeof(double), typeof(MainPage),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToHeightProperty = DependencyProperty.Register(nameof(MoveToHeight), typeof(double), typeof(MainPage),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveRectangeSeekTimeProperty = DependencyProperty.Register(nameof(MoveRectangeSeekTime), typeof(KeyTime), typeof(MainPage),
            new PropertyMetadata(KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
        public static DependencyProperty MoveRectangeSettleTimeProperty = DependencyProperty.Register(nameof(MoveRectangeSettleTime), typeof(KeyTime), typeof(MainPage),
            new PropertyMetadata(KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.25))));

        private ApplicationModel _model;

        private ApplicationDemo _demo;

        private bool _demoMode;

        private bool _demoMovementAnimation;

        private List<TileSequence> _tutorScript;

        public MainPage()
        {
            this.InitializeComponent();

            TemplateConverter.LoadTemplates(Resources);

            SizeChanged += MainWindow_SizeChanged;

            _switchTimer.Tick += OnSwitchTimerTick;
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = (MainPage)d;
            var model = (ApplicationModel)e.NewValue;

            if (page._model != null)
            {
                page._model.ApplicationModelUpdate -= page.OnApplicationModelUpdate;
            }

            page._model = model;

            if (page._model != null)
            {
                page._model.ApplicationModelUpdate += page.OnApplicationModelUpdate;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var passedEnvironment = e.Parameter as IWriterEnvironment;

            var environment = passedEnvironment ?? new UwpWriterEnvironment();
            _model = new ApplicationModel(environment);
            _demo = ApplicationDemo.Create(this);
            var vocalizer = NarratorVocalization.Create(TheMediaElement, "en");
            _ = Narrator.AttachNarrator(_model, vocalizer);

            if (e.Parameter != null && passedEnvironment == null)
            {
                IsEnabled = false;
                try
                {
                    await _model.LoadUtterancesAsync();
                }
                finally
                {
                    IsEnabled = true;
                }
            }
            SetMaxNextSuggestionsCount();
            Model = _model;
        }

        public double MoveToCenterX
        {
            get { return (double)GetValue(MoveToCenterXProperty); }
            set { SetValue(MoveToCenterXProperty, value); }
        }

        public double MoveToCenterY
        {
            get { return (double)GetValue(MoveToCenterYProperty); }
            set { SetValue(MoveToCenterYProperty, value); }
        }

        public double MoveToX
        {
            get { return (double)GetValue(MoveToXProperty); }
            set { SetValue(MoveToXProperty, value); }
        }

        public double MoveToY
        {
            get { return (double)GetValue(MoveToYProperty); }
            set { SetValue(MoveToYProperty, value); }
        }

        public double MoveToWidth
        {
            get { return (double)GetValue(MoveToWidthProperty); }
            set { SetValue(MoveToWidthProperty, value); }
        }

        public double MoveToHeight
        {
            get { return (double)GetValue(MoveToHeightProperty); }
            set { SetValue(MoveToHeightProperty, value); }
        }

        public KeyTime MoveRectangeSeekTime
        {
            get => (KeyTime)GetValue(MoveRectangeSeekTimeProperty);
            set => SetValue(MoveRectangeSeekTimeProperty, value);
        }

        TimeSpan IApplicationHost.MoveRectangeSeekTimeSpan
        {
            get => MoveRectangeSeekTime.TimeSpan;
            set => MoveRectangeSeekTime = value;
        }

        public KeyTime MoveRectangeSettleTime
        {
            get => (KeyTime)GetValue(MoveRectangeSettleTimeProperty);
            set => SetValue(MoveRectangeSettleTimeProperty, value);
        }

        TimeSpan IApplicationHost.MoveRectangeSettleTimeSpan
        {
            get => MoveRectangeSettleTime.TimeSpan;
            set => MoveRectangeSettleTime = value;
        }

        private void ShowDemo(params string[] sentences)
        {
            var script = new List<TileSequence>(sentences.Length);
            foreach (var sentence in sentences)
            {
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    var sequence = TileSequence.FromRaw(sentence);
                    script.Add(sequence);
                }
            }
            ShowDemo(script);
        }

        void IApplicationHost.ShowTargetOutline()
        {
            TargetOutline.Visibility = Visibility.Visible;
        }

        void IApplicationHost.HideTargetOutline()
        {
            TargetOutline.Visibility = Visibility.Collapsed;
        }

        private async void ShowDemo(List<TileSequence> sentences)
        {
            if (_demoMode)
            {
                _demoMode = false;
            }
            else
            {
                _demoMode = true;
                _demoMovementAnimation = true;

                for (var i = 0; _demoMode && i < sentences.Count; i++)
                {
                    TargetOutline.Visibility = Visibility.Visible;

                    var sequence = sentences[i];

                    bool done;
                    do
                    {
                        var action = ApplicationRobot.GetNextCompletionAction(Model, sequence);

                        ((IApplicationHost)this).SetupStoryboardForAction(action);

                        if (_demoMovementAnimation)
                        {
                            await PlayStoryboardAsync(MoveRectangle);
                        }

                        var reaction = ApplicationRobot.GetNextCompletionAction(Model, sequence);
                        if (action.Target == reaction.Target &&
                            action.Index == reaction.Index &&
                            action.SubIndex == reaction.SubIndex)
                        {
                            action.ExecuteItem(Model);
                        }
                        await Task.Delay(TimeSpan.FromSeconds(0.1));

                        done = action.IsComplete;
                    }
                    while (_demoMode && !done);
                }

                _demoMode = false;
                TargetOutline.Visibility = Visibility.Collapsed;
            }
        }

        Task IApplicationHost.PlayMoveRectangleAsync() => PlayStoryboardAsync(MoveRectangle);

        Task IApplicationHost.PlayTutorMoveStoryboardAsync() => PlayStoryboardAsync(TutorMoveStoryboard);

        void IApplicationHost.SetupStoryboardForAction(ApplicationRobotAction action)
        {
            var control = GetActionControl(action);
            Rect targetRect = GetElementRect(control);

            MoveToCenterX = targetRect.X + control.ActualWidth / 2;
            MoveToCenterY = targetRect.Y + control.ActualHeight / 2;
            MoveToX = targetRect.X;
            MoveToY = targetRect.Y;
            MoveToWidth = targetRect.Width;
            MoveToHeight = targetRect.Height;
        }

        private Rect GetElementRect(FrameworkElement control)
        {
            var transform = control.TransformToVisual(TargetPanel);
            var controlRect = new Rect(0, 0, control.ActualWidth, control.ActualHeight);
            var targetRect = transform.TransformBounds(controlRect);
            return targetRect;
        }

        private FrameworkElement GetActionControl(ApplicationRobotAction action)
        {
            UIElement target;
            switch (action.Target)
            {
                case ApplicationRobotActionTarget.Head:
                    target = GetHeadElement(action.Index);
                    break;

                case ApplicationRobotActionTarget.Tail:
                    target = GetTailElement(action.Index);
                    break;

                case ApplicationRobotActionTarget.Interstitial:
                    target = GetInterstitialElement(action.Index);
                    break;

                default:
                case ApplicationRobotActionTarget.Suggestion:
                    Debug.Assert(action.Target == ApplicationRobotActionTarget.Suggestion);
                    target = GetSuggestionElement(action.Index, action.SubIndex);
                    break;

            }
            var targetControl = (FrameworkElement)target;
            return targetControl;
        }

        private FrameworkElement GetSuggestionElement(int index, int subIndex)
        {
            var control = SuggestionListsContainer.ItemsPanelRoot;
            var intermediateTarget = (ContentPresenter)control.Children[index];
            var nextLevel = (ItemsControl)VisualTreeHelper.GetChild(intermediateTarget, 0);
            var nextPanel = nextLevel.ItemsPanelRoot;
            var target = nextPanel.Children[subIndex];
            return (FrameworkElement)target;
        }

        private FrameworkElement GetInterstitialElement(int index)
        {
            var panel = SuggestionInterstitialsContainer.ItemsPanelRoot;
            var target = panel.Children[index];
            return (FrameworkElement)target;
        }

        private FrameworkElement GetTailElement(int index)
        {
            var panel = TailItemsContainer.ItemsPanelRoot;
            var target = panel.Children[index];
            return (FrameworkElement)target;
        }

        private FrameworkElement GetHeadElement(int index)
        {
            var panel = HeadItemsContainer.ItemsPanelRoot;
            var target = panel.Children[index];
            return (FrameworkElement)target;
        }

        private static bool GetAs<T>(Timeline timeline, out T value)
            where T : Timeline
        {
            value = timeline as T;
            return value != null;
        }

        private static TimeSpan GetTimeLineDuration(Timeline timeline)
        {
            var begin = timeline.BeginTime ?? TimeSpan.Zero;
            var duration = timeline.Duration.HasTimeSpan ? timeline.Duration.TimeSpan : TimeSpan.Zero;

            if (GetAs<Storyboard>(timeline, out var storyboard))
            {
                foreach (var child in storyboard.Children)
                {
                    var candidateDuration = GetTimeLineDuration(child);

                    if (duration < candidateDuration)
                    {
                        duration = candidateDuration;
                    }
                }
            }
            else if (GetAs<DoubleAnimationUsingKeyFrames>(timeline, out var doubleAnimationUsingKeyFrames))
            {
                foreach (var doubleKeyFrame in doubleAnimationUsingKeyFrames.KeyFrames)
                {
                    var keyTime = doubleKeyFrame.KeyTime.TimeSpan;

                    if (duration < keyTime)
                    {
                        duration = keyTime;
                    }
                }
            }
            else if (timeline is DoubleAnimation)
            {
                // Nothing to do.
            }
            else
            {
                duration = TimeSpan.FromSeconds(1);

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            return begin + duration;
        }

        private static async Task PlayStoryboardAsync(Storyboard storyboard)
        {
            var semaphore = new SemaphoreSlim(0);
            void handler(object s, object e) => semaphore.Release();
            storyboard.Completed += handler;
            try
            {
                var expectedDuration = (int)(GetTimeLineDuration(storyboard).TotalMilliseconds + 0.5);

                var startTick = Environment.TickCount;
                storyboard.Begin();

                var completed = await semaphore.WaitAsync(expectedDuration + 500);
                var actualDuration = Environment.TickCount - startTick;

                if (completed)
                {
                    Debug.WriteLine($"Storyboard completed, measured at {actualDuration}ms");
                }
                else
                {
                    Debug.WriteLine($"Missed animation completion, measured at {actualDuration}ms");
                }
            }
            finally
            {
                storyboard.Completed -= handler;
            }
        }

        private void OnApplicationModelUpdate(object sender, ApplicationModelUpdateEventArgs e)
        {
            if (_switchMode)
            {
                _ = ShowSwitchInterfaceAsync();
            }
        }

        public ApplicationModel Model
        {
            get => (ApplicationModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        private void SetMaxNextSuggestionsCount()
        {
            var count = Math.Max(1, (int)(ActualHeight) / 110);
            _model.MaxNextSuggestionsCount = count;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Model != null)
            {
                SetMaxNextSuggestionsCount();
            }
        }

        private void OnRestart(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            Frame.Navigate(GetType(), string.Empty);
        }

        void IApplicationHost.Restart(bool loadHistory) => Frame.Navigate(GetType(), loadHistory ? string.Empty : null);

        private void OnClickKirk(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            ShowDemo("space",
                "the final frontier",
                "these are the voyages of the starship Enterprise",
                "its five year mission",
                "to explore strange new worlds",
                "to seek out new life",
                "and new civilizations",
                "to boldly go where no man has gone before");
        }

        private void OnClickPicard(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            ShowDemo("these are the voyages of the starship Enterprise",
                "its continuing mission",
                "to explore strange new worlds",
                "to seek out new life",
                "and new civilizations",
                "to boldly go where no one has gone before");
        }

        private async void OnPaste(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            var script = await GetClipboardContentAsync();

            if (script.Count != 0)
            {
                ShowDemo(script);
            }
        }

        private async void OnClickTutor(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;

            _demoMode = false;

            var script = await GetClipboardContentAsync();
            if (script.Count != 0)
            {
                _tutorScript = script;
                TargetOutline.Visibility = Visibility.Visible;

                _model.ApplicationModelUpdate += OnApplicationTutorReady;
                _ = ShowNextTutorStepAsync();
            }
        }

        private async void OnApplicationTutorReady(object sender, ApplicationModelUpdateEventArgs e)
        {
            if (_tutorScript != null)
            {
                if (e.IsComplete /* && string.Join(' ', e.Words) == _tutorScript[0] */ )
                {
                    if (_tutorScript.Count == 1)
                    {
                        _tutorScript = null;
                        _model.ApplicationModelUpdate -= OnApplicationTutorReady;

                        TargetOutline.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        _tutorScript.RemoveAt(0);
                        await ShowNextTutorStepAsync();
                    }
                }
                else
                {
                    await ShowNextTutorStepAsync();
                }
            }
            else
            {
                _model.ApplicationModelUpdate -= OnApplicationTutorReady;
            }
        }

        private async Task ShowNextTutorStepAsync()
        {
            await Task.Delay(50);
            var words = _tutorScript[0];
            var action = ApplicationRobot.GetNextCompletionAction(_model, words);
            ((IApplicationHost)this).SetupStoryboardForAction(action);
            _ = PlayStoryboardAsync(TutorMoveStoryboard);
        }

        private async Task<List<TileSequence>> GetClipboardContentAsync()
        {
            var script = new List<TileSequence>();

            string text = await ((IApplicationHost)this).GetClipboardStringAsync();

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var utterance = new List<TileData>();

                var sequence = TileSequence.FromRaw(line);

                var isUtteranceEnding = false;
                foreach (var tile in sequence)
                {
                    utterance.Add(tile);

                    switch (tile.Content)
                    {
                        case ".":
                        case "?":
                        case "!":
                            isUtteranceEnding = true;
                            break;
                    }

                    if (isUtteranceEnding && !tile.IsPrefix)
                    {
                        var utteranceSequence = TileSequence.FromData(utterance);
                        script.Add(utteranceSequence);
                        utterance.Clear();

                        isUtteranceEnding = false;
                    }
                }

                if (utterance.Count != 0)
                {
                    var utteranceSequence = TileSequence.FromData(utterance);
                    script.Add(utteranceSequence);
                }
            }

            return script;
        }

        async Task<string> IApplicationHost.GetClipboardStringAsync()
        {
            var view = Clipboard.GetContent();
            var text = await view.GetTextAsync();
            return text;
        }

        private void OnClickQuick(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            _demoMovementAnimation = false;
        }

        private async void SetLanguageAsync(string filename)
        {
            var voiceChoices = new List<object>();
            foreach (var voice in SpeechSynthesizer.AllVoices)
            {
                if (voice.Language.Substring(0, 2) == filename.Substring(0, 2))
                {
                    voiceChoices.Add(voice);
                }
            }

            var uri = new Uri($"ms-appx:///Assets/{filename}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var content = await FileIO.ReadTextAsync(file);
            var environment = new NonEnglishWriterEnvironment(filename.Substring(0, 2), content);
            Frame.Navigate(typeof(MainPage), environment);
        }

        private void OnClickReset(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            Frame.Navigate(GetType(), null);
        }

        private void OnFrench(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("fr_50k.txt");
        }

        private void OnSpanish(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("es_50k.txt");
        }

        private void OnGerman(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("de_50k.txt");
        }

        private void OnThai(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("th_50k.txt");
        }

        private void OnPortuguese(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("pt_br_50k.txt");
        }

        private void OnItalian(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("it_50k.txt");
        }

        private void OnArabic(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("ar_50k.txt");
        }

        private void OnChinese(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SetLanguageAsync("zh_cn_50k.txt");
        }

        private void OnTimingChange(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (MoveRectangeSeekTime.TimeSpan.TotalSeconds == 1)
            {
                MoveRectangeSeekTime = TimeSpan.FromSeconds(0.1);
                MoveRectangeSettleTime = TimeSpan.FromSeconds(0.5);
            }
            else
            {
                MoveRectangeSeekTime = TimeSpan.FromSeconds(1);
                MoveRectangeSettleTime = TimeSpan.FromSeconds(1.25);
            }
        }

        private async void OnShowLogging(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var environment = Model.Environment as UwpWriterEnvironment;
            if (environment != null)
            {
                var file = await environment.GetHistoryFileAsync();
                var path = file.Path;
                var package = new DataPackage();
                package.SetText(path);
                Clipboard.SetContent(package);
            }
        }

        void IApplicationHost.ShowLogging() => OnShowLogging(null, null);

        private void OnShowTestCard(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            Model.ShowTestCard();
        }

        private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _demo.DoSpecialKey(e.Key);
        }
    }
}
