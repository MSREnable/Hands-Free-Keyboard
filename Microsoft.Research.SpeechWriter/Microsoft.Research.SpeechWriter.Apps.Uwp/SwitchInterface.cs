using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using Microsoft.Research.SpeechWriter.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Research.SpeechWriter.Apps.Uwp
{
    public partial class SwitchInterface
    {
        private readonly ApplicationModel _model;
        private readonly IApplicationHost _host;
        private readonly ISuperPanel<FrameworkElement, Size, Rect> _panel;
        private readonly Canvas SwitchPanel;
        private ApplicationRobotActionTarget _switchTarget;
        private int _switchClickCount;
        private readonly DispatcherTimer _switchTimer = new DispatcherTimer();
        private readonly List<SwitchTargetControl> _targets = new List<SwitchTargetControl>();
        private int _switchSuggestionListsIndex;

        internal SwitchInterface(IApplicationHost host, ISuperPanel<FrameworkElement, Size, Rect> panel, Canvas targetCanvas)
        {
            _host = host;
            _panel = panel;
            _model = host.Model;
            SwitchPanel = targetCanvas;

            _switchTimer.Tick += OnSwitchTimerTick;

            _model.ApplicationModelUpdate += OnApplicationModelUpdate;

            Debug.WriteLine("Enter switch mode");
            _switchTarget = ApplicationRobotActionTarget.Interstitial;
            ShowSwitchInterface();
        }

        private void OnApplicationModelUpdate(object sender, ApplicationModelUpdateEventArgs e)
        {
            _ = ShowSwitchInterfaceAsync();
        }

        private void ShowSwitchInterface()
        {
            _switchClickCount = 0;
            _switchTimer.Interval = TimeSpan.FromSeconds(10);
            _switchTimer.Start();

            SwitchPanel.Children.Clear();
            _targets.Clear();

            switch (_switchTarget)
            {
                case ApplicationRobotActionTarget.Head:
                    {
                        AddSwitchToInterstitials();

                        AddSwitchToSuggestions();

                        for (var index = 0; index < _model.HeadItems.Count; index++)
                        {
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Head, index, 0, false);
                            AddRectangle(element);
                        }

                        for (var index = 0; index < _model.TailItems.Count; index++)
                        {
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, index, 0, false);
                            AddRectangle(element);
                        }
                    }
                    break;

                case ApplicationRobotActionTarget.Tail:
                    {
                        AddSwitchToSuggestions();

                        var list = _model.SuggestionLists[_switchSuggestionListsIndex];
                        for (var subIndex = 0; subIndex < list.Count(); subIndex++)
                        {
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, _switchSuggestionListsIndex, subIndex, false);
                            AddRectangle(element);
                        }
                    }
                    break;

                case ApplicationRobotActionTarget.Suggestion:
                    {
                        AddSwitchToHead();

                        AddSwitchToInterstitials();

                        for (var index = 0; index < _model.SuggestionLists.Count; index++)
                        {
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, 0, false);

                            var list = _model.SuggestionLists[index];
                            var count = list.Count();

                            if (1 < count)
                            {
                                var lastElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, count - 1, false);

                                var uncapturedIndex = index;
                                AddRectangle(() => { _switchTarget = ApplicationRobotActionTarget.Tail; _switchSuggestionListsIndex = uncapturedIndex; ShowSwitchInterface(); }, element, lastElement);
                            }
                            else
                            {
                                AddRectangle(element);
                            }
                        }
                    }
                    break;

                case ApplicationRobotActionTarget.Interstitial:
                default:
                    {
                        Debug.Assert(_switchTarget == ApplicationRobotActionTarget.Interstitial);

                        AddSwitchToSuggestions();

                        AddSwitchToHead();

                        for (var index = 0; index < _model.SuggestionInterstitials.Count; index++)
                        {
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, index, 0, false);
                            AddRectangle(element);
                        }
                    }
                    break;
            }
        }

        private void AddSwitchToInterstitials()
        {
            AddRectangle(() => { _switchTarget = ApplicationRobotActionTarget.Interstitial; ShowSwitchInterface(); },
                new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, 0, 0, false),
                new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, _model.SuggestionInterstitials.Count - 1, 0, false));
        }

        private void AddSwitchToHead()
        {
            var headElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Head, 0, 0, false);
            var tailElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, _model.TailItems.Count - 1, 0, false);

            AddRectangle(() => { _switchTarget = ApplicationRobotActionTarget.Head; ShowSwitchInterface(); }, headElement, tailElement);
        }

        private void AddSwitchToSuggestions()
        {
            var firstElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, 0, 0, false);

            var actions = new List<ApplicationRobotAction>();
            for (var index = 1; index < _model.SuggestionLists.Count; index++)
            {
                var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, 0, false);
                actions.Add(element);
            }

            for (var index = 0; index < _model.SuggestionLists.Count; index++)
            {
                var list = _model.SuggestionLists[index];
                var count = list.Count();
                if (1 < count)
                {
                    var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, list.Count() - 1, false);
                    actions.Add(element);
                }
            }

            AddRectangle(() => { _switchTarget = ApplicationRobotActionTarget.Suggestion; ShowSwitchInterface(); }, firstElement, actions.ToArray());
        }

        private async Task ShowSwitchInterfaceAsync()
        {
            await Task.Delay(200);
            _switchTarget = ApplicationRobotActionTarget.Interstitial;
            ShowSwitchInterface();
        }

        private void AddRectangle(Action action, Rect overallRect)
        {
            var target = new SwitchTargetControl
            {
                Index = _targets.Count + 1,
                Width = overallRect.Width,
                Height = overallRect.Height,
                Action = action
            };
            Canvas.SetLeft(target, overallRect.Left);
            Canvas.SetTop(target, overallRect.Top);

            SwitchPanel.Children.Add(target);
            _targets.Add(target);
        }

        private void AddRectangle(Action action, ApplicationRobotAction robotAction, params ApplicationRobotAction[] otherRobotActions)
        {
            var overallRect = _panel.GetTargetRect(SwitchPanel, robotAction);
            foreach (var otherRobotAction in otherRobotActions)
            {
                var otherRect = _panel.GetTargetRect(SwitchPanel, otherRobotAction);
                overallRect.Union(otherRect);
            }

            AddRectangle(action, overallRect);
        }

        private void AddRectangle(ApplicationRobotAction robotAction)
        {

            Action action = () => robotAction.ExecuteItem(_model);
            AddRectangle(action, robotAction);
        }

        internal void OnSpace()
        {
            _switchTimer.Stop();

            if (_switchClickCount != 0)
            {
                var oldIndex = (_switchClickCount - 1) % _targets.Count;
                Debug.Assert(_targets[oldIndex].IsSelected);
                _targets[oldIndex].IsSelected = false;
            }

            _switchClickCount++;
            _switchTimer.Interval = TimeSpan.FromSeconds(2);
            _switchTimer.Start();

            Debug.WriteLine($"Clicked to {_switchClickCount}");

            var index = (_switchClickCount - 1) % _targets.Count;
            Debug.Assert(!_targets[index].IsSelected);
            _targets[index].IsSelected = true;

            _targets[index].HighlightColor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
        }
        private void OnSwitchTimerTick(object sender, object e)
        {
            _switchTimer.Stop();
            if (_switchClickCount == 0)
            {
                Debug.WriteLine("Exit switch mode");
                SwitchPanel.Children.Clear();
                _host.EndSwitchMode();
            }
            else
            {
                Debug.WriteLine($"Clicked {_switchClickCount} positions");
                var index = (_switchClickCount - 1) % _targets.Count;
                var target = _targets[index];
                target.Action();
            }
        }
    }
}
