using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using Microsoft.Research.SpeechWriter.UI;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Research.SpeechWriter.Apps.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Page, IApplicationHost
    {
        public static DependencyProperty MoveToCenterXProperty = DependencyProperty.Register(nameof(MoveToCenterX), typeof(double), typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToCenterYProperty = DependencyProperty.Register(nameof(MoveToCenterY), typeof(double), typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToXProperty = DependencyProperty.Register(nameof(MoveToX), typeof(double), typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToYProperty = DependencyProperty.Register(nameof(MoveToY), typeof(double), typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToWidthProperty = DependencyProperty.Register(nameof(MoveToWidth), typeof(double), typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveToHeightProperty = DependencyProperty.Register(nameof(MoveToHeight), typeof(double), typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static DependencyProperty MoveRectangeSeekTimeProperty = DependencyProperty.Register(nameof(MoveRectangeSeekTime), typeof(KeyTime), typeof(MainWindow),
            new PropertyMetadata(KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))));
        public static DependencyProperty MoveRectangeSettleTimeProperty = DependencyProperty.Register(nameof(MoveRectangeSettleTime), typeof(KeyTime), typeof(MainWindow),
            new PropertyMetadata(KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.25))));

        private static bool _loadHistory = true;

        private readonly ApplicationModel _model;

        private readonly ApplicationDemo _demo;

        public MainWindow()
        {
            var environment = new WpfEnvironment();
            _model = new ApplicationModel(environment);

            DataContext = this;

            InitializeComponent();

            TheContent.DataContext = _model;

            TheContent.SizeChanged += MainWindow_SizeChanged;

            var vocalizer = new NarratorVocalizer();
            _ = Narrator.AttachNarrator(_model, vocalizer);

            Loaded += OnLoaded;

            _demo = ApplicationDemo.Create(this);
        }

        public ApplicationModel Model => _model;

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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;
            Focus();
            if (_loadHistory)
            {
                await _model.LoadUtterancesAsync();
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var head = GetTargetRect(new ApplicationRobotAction(ApplicationRobotActionTarget.Head, 0, 0, false));
            var tail = GetTargetRect(new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, 0, 0, false));
            var interstitial = GetTargetRect(new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, 0, 0, false));
            var suggestion = GetTargetRect(new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, 0, 0, false));
            _model.MaxNextSuggestionsCount = (int)(e.NewSize.Height) / 60;
        }

        private Rect GetTargetRect(ApplicationRobotAction action)
        {
            var control = GetActionControl(action);
            var targetRect = GetElementRect(control);
            return targetRect;
        }

        void IApplicationHost.SetupStoryboardForAction(ApplicationRobotAction action)
        {
            var targetRect = GetTargetRect(action);

            MoveToCenterX = targetRect.X + targetRect.Width / 2;
            MoveToCenterY = targetRect.Y + targetRect.Height / 2;
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

        private T GetPanel<T>(ItemsControl itemsContainer)
            where T : Panel
        {
            var border = (Border)VisualTreeHelper.GetChild(itemsContainer, 0);
            var presenter = (ItemsPresenter)border.Child;
            var panel = (T)VisualTreeHelper.GetChild(presenter, 0);
            return panel;
        }

        private UIElement GetSuggestionElement(int index, int subIndex)
        {
            var outerStack = GetPanel<StackPanel>(SuggestionListsContainer);
            var untypedInnerPresenter = (ContentPresenter)outerStack.Children[index];
            var wibble = VisualTreeHelper.GetChild(untypedInnerPresenter, 0);
            var innerPresenter = (ItemsControl)wibble;
            var innerStack = GetPanel<StackPanel>(innerPresenter);
            var target = innerStack.Children[subIndex];
            return target;
        }

        void IApplicationHost.ShowLogging()
        {
            Process.Start("explorer.exe", WpfEnvironment.DataPath);
        }

        private UIElement GetInterstitialElement(int index)
        {
            var stack = GetPanel<StackPanel>(SuggestionInterstitialsContainer);
            var target = stack.Children[index];
            return target;
        }

        private UIElement GetTailElement(int index)
        {
            var stack = GetPanel<WrapPanel>(TailItemsContainer);
            var target = stack.Children[index];
            return target;
        }

        private FrameworkElement GetHeadElement(int index)
        {
            var border = (Border)VisualTreeHelper.GetChild(HeadItemsContainer, 0);
            var presenter = (ItemsPresenter)border.Child;
            var panel = (WrapPanel)VisualTreeHelper.GetChild(presenter, 0);
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
                foreach (DoubleKeyFrame doubleKeyFrame in doubleAnimationUsingKeyFrames.KeyFrames)
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

        private static async Task PlayStoryboardAsync(Storyboard storyboard, TimeSpan expectedDurationTs)
        {
            var semaphore = new SemaphoreSlim(0);
            void handler(object s, object e) => semaphore.Release();
            storyboard.Completed += handler;
            try
            {
                var expectedDuration = (int)(expectedDurationTs.TotalMilliseconds + 0.5);

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

        async Task IApplicationHost.PlayMoveRectangleAsync()
        {
            var storyboard = (Storyboard)Resources["MoveRectangle"];
            await PlayStoryboardAsync(storyboard, MoveRectangeSettleTime.TimeSpan);
        }

        async Task IApplicationHost.PlayTutorMoveStoryboardAsync()
        {
            var storyboard = (Storyboard)Resources["TutorMoveStoryboard"];
            await PlayStoryboardAsync(storyboard, MoveRectangeSettleTime.TimeSpan);
        }

        void IApplicationHost.ShowTargetOutline()
        {
            TargetOutline.Visibility = Visibility.Visible;
        }

        void IApplicationHost.HideTargetOutline()
        {
            TargetOutline.Visibility = Visibility.Collapsed;
        }

        public void Restart(bool loadHistory)
        {
            _loadHistory = loadHistory;
            NavigationService.Navigate(new MainWindow());
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_demo.DoSpecialKey(e.Key))
            {
                e.Handled = true;
            }
        }

        Task<string> IApplicationHost.GetClipboardStringAsync()
        {
            var text = Clipboard.GetText();
            return Task.FromResult(text);
        }
    }
}
