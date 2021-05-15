using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    public sealed partial class MainPage
    {
        private bool _switchMode;
        private ApplicationRobotActionTarget _switchTarget;
        private int _switchClickCount;
        private readonly DispatcherTimer _switchTimer = new DispatcherTimer();
        private readonly List<SwitchTargetControl> _targets = new List<SwitchTargetControl>();
        private int _switchSuggestionListsIndex;

        private void ShowSwitchInterface()
        {
            _switchMode = true;
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
                            var element = GetHeadElement(index);
                            var rect = GetElementRect(element);

                            var uncapturedIndex = index;
                            AddRectangle(rect, () => new ApplicationRobotAction(ApplicationRobotActionTarget.Head, uncapturedIndex, 0, false).ExecuteItem(_model));
                        }

                        for (var index = 0; index < _model.TailItems.Count; index++)
                        {
                            var element = GetTailElement(index);
                            var rect = GetElementRect(element);

                            var uncapturedIndex = index;
                            AddRectangle(rect, () => new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, uncapturedIndex, 0, false).ExecuteItem(_model));
                        }
                    }
                    break;

                case ApplicationRobotActionTarget.Tail:
                    {
                        AddSwitchToSuggestions();

                        var list = _model.SuggestionLists[_switchSuggestionListsIndex];
                        for (var subIndex = 0; subIndex < list.Count(); subIndex++)
                        {
                            var element = GetSuggestionElement(_switchSuggestionListsIndex, subIndex);
                            var rect = GetElementRect(element);

                            var uncapturedIndex = subIndex;
                            AddRectangle(rect, () => new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, _switchSuggestionListsIndex, uncapturedIndex, false).ExecuteItem(_model));
                        }
                    }
                    break;

                case ApplicationRobotActionTarget.Suggestion:
                    {
                        AddSwitchToHead();

                        AddSwitchToInterstitials();

                        for (var index = 0; index < _model.SuggestionLists.Count; index++)
                        {
                            var element = GetSuggestionElement(index, 0);
                            var rect = GetElementRect(element);

                            var list = _model.SuggestionLists[index];
                            var count = list.Count();

                            Action action;
                            if (1 < count)
                            {
                                var lastElement = GetSuggestionElement(index, count - 1);
                                var lastRect = GetElementRect(lastElement);
                                rect.Union(lastRect);

                                var uncapturedIndex = index;
                                action = () => { _switchTarget = ApplicationRobotActionTarget.Tail; _switchSuggestionListsIndex = uncapturedIndex; ShowSwitchInterface(); };
                            }
                            else
                            {
                                var uncapturedIndex = index;
                                action = () => new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, uncapturedIndex, 0, false).ExecuteItem(_model);
                            }

                            AddRectangle(rect, action);
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
                            var element = GetInterstitialElement(index);
                            var rect = GetElementRect(element);
                            var uncapturedIndex = index;
                            AddRectangle(rect, () => new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, uncapturedIndex, 0, false).ExecuteItem(_model));
                        }
                    }
                    break;
            }
        }

        private void AddSwitchToInterstitials()
        {
            var overallRect = Rect.Empty;
            for (var index = 0; index < _model.SuggestionInterstitials.Count; index++)
            {
                var element = GetInterstitialElement(index);
                var rect = GetElementRect(element);
                overallRect.Union(rect);
            }

            AddRectangle(overallRect, () => { _switchTarget = ApplicationRobotActionTarget.Interstitial; ShowSwitchInterface(); });
        }

        private void AddSwitchToHead()
        {
            var headElement = GetHeadElement(0);
            var overallRect = GetElementRect(headElement);

            var tailElement = GetTailElement(_model.TailItems.Count - 1);
            var tailRect = GetElementRect(tailElement);
            overallRect.Union(tailRect);

            AddRectangle(overallRect, () => { _switchTarget = ApplicationRobotActionTarget.Head; ShowSwitchInterface(); });
        }

        private void AddSwitchToSuggestions()
        {
            var overallRect = Rect.Empty;
            for (var index = 0; index < _model.SuggestionLists.Count; index++)
            {
                var firstElement = GetSuggestionElement(index, 0);
                var firstRect = GetElementRect(firstElement);
                overallRect.Union(firstRect);

                var list = _model.SuggestionLists[index];
                var count = list.Count();
                if (1 < count)
                {
                    var lastElement = GetSuggestionElement(index, list.Count() - 1);
                    var lastRect = GetElementRect(lastElement);
                    overallRect.Union(lastRect);
                }
            }

            AddRectangle(overallRect, () => { _switchTarget = ApplicationRobotActionTarget.Suggestion; ShowSwitchInterface(); });
        }

        private async Task ShowSwitchInterfaceAsync()
        {
            await Task.Delay(200);
            _switchTarget = ApplicationRobotActionTarget.Interstitial;
            ShowSwitchInterface();
        }

        private void AddRectangle(Rect overallRect, Action action)
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

        private void OnSpace(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;

            _switchTimer.Stop();

            if (!_switchMode)
            {
                Debug.WriteLine("Enter switch mode");
                _switchTarget = ApplicationRobotActionTarget.Interstitial;
                ShowSwitchInterface();
            }
            else
            {
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
        }
        private void OnSwitchTimerTick(object sender, object e)
        {
            _switchTimer.Stop();
            if (_switchClickCount == 0)
            {
                Debug.WriteLine("Exit switch mode");
                _switchMode = false;
                SwitchPanel.Children.Clear();
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
