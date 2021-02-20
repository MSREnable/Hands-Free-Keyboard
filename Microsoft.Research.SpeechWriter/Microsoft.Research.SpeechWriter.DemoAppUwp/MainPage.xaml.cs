using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.SpeechSynthesis;
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
    public sealed partial class MainPage : Page
    {
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

        private class EmptyEnvironment : DefaultWriterEnvironment, IWriterEnvironment
        {
            /// <summary>
            /// Dictionary of words, listed from most likely to least likely.
            /// </summary>
            /// <returns>List of words.</returns>
            public IEnumerable<string> GetOrderedSeedWords()
            {
                return new string[0];
            }
        }

        private readonly ApplicationModel _model;

        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        private readonly Queue<ApplicationModelUpdateEventArgs> _speech = new Queue<ApplicationModelUpdateEventArgs>();

        private readonly SemaphoreSlim _mediaReady = new SemaphoreSlim(1);

        private bool _demoMode;

        private bool _demoMovementAnimation;

        private List<string> _tutorScript;

        public MainPage()
        {
            this.InitializeComponent();

            var environment = new UwpWriterEnvironment();
            _model = new ApplicationModel(environment);

            TemplateConverter.LoadTemplates(Resources);

            SizeChanged += MainWindow_SizeChanged;
            _model.ApplicationModelUpdate += OnCollectionChanged;

            TheMediaElement.MediaEnded += (s, e) => _mediaReady.Release();

            _ = ConsumeSpeechAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
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

        private async void ShowDemo(params string[] sentences)
        {
            if (_demoMode)
            {
                _demoMode = false;
            }
            else
            {
                _demoMode = true;
                _demoMovementAnimation = true;

                for (var i = 0; _demoMode && i < sentences.Length; i++)
                {
                    TargetOutline.Visibility = Visibility.Visible;

                    var words = sentences[i].Split(' ');

                    bool done;
                    do
                    {
                        var action = ApplicationRobot.GetNextCompletionAction(Model, words);

                        SetupStoryboardForAction(action);

                        if (_demoMovementAnimation)
                        {
                            await PlayStoryboardAsync(MoveRectangle);
                        }

                        var reaction = ApplicationRobot.GetNextCompletionAction(Model, words);
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

        private void SetupStoryboardForAction(ApplicationRobotAction action)
        {
            UIElement target;
            switch (action.Target)
            {
                case ApplicationRobotActionTarget.Head:
                    {
                        var control = HeadItemsContainer.ItemsPanelRoot;
                        target = control.Children[action.Index];
                    }
                    break;

                case ApplicationRobotActionTarget.Tail:
                    {
                        var control = TailItemsContainer.ItemsPanelRoot;
                        target = control.Children[action.Index];
                    }
                    break;

                case ApplicationRobotActionTarget.Interstitial:
                    {
                        var control = SuggestionInterstitialsContainer.ItemsPanelRoot;
                        target = control.Children[action.Index];
                    }
                    break;

                default:
                case ApplicationRobotActionTarget.Suggestion:
                    {
                        var control = SuggestionListsContainer.ItemsPanelRoot;
                        var intermediateTarget = (ContentPresenter)control.Children[action.Index];
                        var nextLevel = (ItemsControl)VisualTreeHelper.GetChild(intermediateTarget, 0);
                        var nextPanel = nextLevel.ItemsPanelRoot;
                        target = nextPanel.Children[action.SubIndex];
                    }
                    break;

            }
            var targetControl = (ContentPresenter)target;
            var transform = targetControl.TransformToVisual(TargetPanel);
            var targetPoint = transform.TransformPoint(new Windows.Foundation.Point(0, 0));

            MoveToCenterX = targetPoint.X + targetControl.ActualWidth / 2;
            MoveToCenterY = targetPoint.Y + targetControl.ActualHeight / 2;
            MoveToX = targetPoint.X;
            MoveToY = targetPoint.Y;
            MoveToWidth = targetControl.ActualWidth;
            MoveToHeight = targetControl.ActualHeight;
        }

        private static bool GetAs<T>(Timeline timeline, out T value)
            where T : Timeline
        {
            value = timeline as T;
            return value != null;
        }

        private static TimeSpan GetTimeLineDuration(Timeline timeline)
        {
            var begin = timeline.BeginTime.HasValue ? timeline.BeginTime.Value : TimeSpan.Zero;
            var duration = timeline.Duration.HasTimeSpan ? timeline.Duration.TimeSpan : TimeSpan.Zero; ;

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
            EventHandler<object> handler = (s, e) => semaphore.Release();
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

        private int sent;

        private void OnCollectionChanged(object sender, ApplicationModelUpdateEventArgs e)
        {
            lock (_speech)
            {
                _speech.Enqueue(e);
                Debug.WriteLine($"Sending item {++sent}");
            }
            _semaphore.Release();
        }

        private async Task ConsumeSpeechAsync()
        {
            var received = 0;

            var spokenDepth = 0;

            for (; ; )
            {
                ApplicationModelUpdateEventArgs e;
                do
                {
                    Debug.WriteLine($"Waiting for {received + 1}");
                    await _semaphore.WaitAsync();

                    lock (_speech)
                    {
                        e = _speech.Dequeue();
                    }

                    Debug.WriteLine($"Got item {++received}");
                }
                while (spokenDepth == e.Words.Count &&
                    !e.IsComplete);

                string text;

                if (e.Words.Count < spokenDepth)
                {
                    Debug.Assert(!e.IsComplete);

                    text = "Uh-oh";
                    spokenDepth = e.Words.Count;
                }
                else
                {
                    if (spokenDepth < e.Words.Count)
                    {
                        text = e.Words[spokenDepth];
                        spokenDepth++;
                        while (spokenDepth < e.Words.Count)
                        {
                            text += " " + e.Words[spokenDepth];
                            spokenDepth++;
                        }
                    }
                    else
                    {
                        text = null;
                    }

                    if (e.IsComplete)
                    {
                        spokenDepth = 0;
                    }
                }

                Debug.WriteLine($"Saying \"{text}\"");

                if (text != null)
                {
                    Debug.WriteLine("Waiting for media");
                    await _mediaReady.WaitAsync();
                    Debug.WriteLine("Media ready");

                    var stream = await _synthesizer.SynthesizeTextToStreamAsync(text.ToLower());
                    TheMediaElement.AutoPlay = true;
                    TheMediaElement.SetSource(stream, stream.ContentType);
                    TheMediaElement.Play();
                }
            }
        }

        public ApplicationModel Model => _model;

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _model.MaxNextSuggestionsCount = (int)(e.NewSize.Height) / 110;
        }

        private void OnClickKirk(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            ShowDemo("SPACE",
                "THE FINAL FRONTIER",
                "THESE ARE THE VOYAGES OF THE STARSHIP ENTERPRISE",
                "ITS FIVE YEAR MISSION",
                "TO EXPLORE STRANGE NEW WORLDS",
                "TO SEEK OUT NEW LIFE",
                "AND NEW CIVILIZATIONS",
                "TO BOLDLY GO WHERE NO MAN HAS GONE BEFORE");
        }

        private void OnClickPicard(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            ShowDemo("THESE ARE THE VOYAGES OF THE STARSHIP ENTERPRISE",
                "ITS CONTINUING MISSION",
                "TO EXPLORE STRANGE NEW WORLDS",
                "TO SEEK OUT NEW LIFE",
                "AND NEW CIVILIZATIONS",
                "TO BOLDLY GO WHERE NO ONE HAS GONE BEFORE");
        }

        private async void OnPaste(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            var script = await GetClipboardContentAsync();

            if (script.Count != 0)
            {
                ShowDemo(script.ToArray());
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
                if (e.IsComplete && string.Join(' ', e.Words) == _tutorScript[0])
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
            var words = _tutorScript[0].Split(' ');
            var action = ApplicationRobot.GetNextCompletionAction(_model, words);
            SetupStoryboardForAction(action);
            _ = PlayStoryboardAsync(TutorMoveStoryboard);
        }

        private static async Task<List<string>> GetClipboardContentAsync()
        {
            var view = Clipboard.GetContent();
            var text = await view.GetTextAsync();
            var upper = text.ToUpper();

            var builder = new StringBuilder();
            foreach (var ch in upper)
            {
                switch (ch)
                {
                    case '\r':
                    case '\n':
                    case '.':
                    case '!':
                    case '?':
                    case ':':
                    case '\'':
                    case '-':
                    case ' ':
                        builder.Append(ch);
                        break;

                    default:
                        if (char.IsLetterOrDigit(ch))
                        {
                            builder.Append(ch);
                        }
                        break;
                }
            }

            var script = new List<string>();
            var lines = builder.ToString().Split(new[] { '\r', '\n', '.', '?', '!', ':' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length != 0)
                {
                    script.Add(string.Join(' ', words));
                }
            }

            return script;
        }

        private void OnClickQuick(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            _demoMovementAnimation = false;
        }

        private void OnClickReset(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            Frame.Navigate(GetType(), null);
        }
    }
}
