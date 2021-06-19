﻿using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Microsoft.Research.SpeechWriter.Apps.Uwp
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
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Head, index, 0, false);
                            var rect = _layout.GetTargetRect(element);

                            AddRectangle(rect, () => element.ExecuteItem(_model));
                        }

                        for (var index = 0; index < _model.TailItems.Count; index++)
                        {
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, index, 0, false);
                            var rect = _layout.GetTargetRect(element);

                            AddRectangle(rect, () => element.ExecuteItem(_model));
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
                            var rect = _layout.GetTargetRect(element);

                            AddRectangle(rect, () => element.ExecuteItem(_model));
                        }
                    }
                    break;

                case ApplicationRobotActionTarget.Suggestion:
                    {
                        AddSwitchToHead();

                        AddSwitchToInterstitials();

                        for (var index = 0; index < _model.SuggestionLists.Count; index++)
                        {
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Head, index, 0, false);
                            var rect = _layout.GetTargetRect(element);

                            var list = _model.SuggestionLists[index];
                            var count = list.Count();

                            Action action;
                            if (1 < count)
                            {
                                var lastElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Head, index, count - 1, false);
                                var lastRect = _layout.GetTargetRect(lastElement);
                                rect = RectangleF.Union(rect, lastRect);

                                var uncapturedIndex = index;
                                action = () => { _switchTarget = ApplicationRobotActionTarget.Tail; _switchSuggestionListsIndex = uncapturedIndex; ShowSwitchInterface(); };
                            }
                            else
                            {
                                var uncapturedIndex = index;
                                action = () => element.ExecuteItem(_model);
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
                            var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, index, 0, false);
                            var rect = _layout.GetTargetRect(element);
                            AddRectangle(rect, () => element.ExecuteItem(_model));
                        }
                    }
                    break;
            }
        }

        private void AddSwitchToInterstitials()
        {
            var overallRect = RectangleF.Empty;
            for (var index = 0; index < _model.SuggestionInterstitials.Count; index++)
            {
                var element = new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, index, 0, false);
                var rect = _layout.GetTargetRect(element);
                overallRect = RectangleF.Union(overallRect, rect);
            }

            AddRectangle(overallRect, () => { _switchTarget = ApplicationRobotActionTarget.Interstitial; ShowSwitchInterface(); });
        }

        private void AddSwitchToHead()
        {
            var headElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Head, 0, 0, false);
            var overallRect = _layout.GetTargetRect(headElement);

            var tailElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, _model.TailItems.Count - 1, 0, false);
            var tailRect = _layout.GetTargetRect(tailElement);
            overallRect = RectangleF.Union(overallRect, tailRect);

            AddRectangle(overallRect, () => { _switchTarget = ApplicationRobotActionTarget.Head; ShowSwitchInterface(); });
        }

        private void AddSwitchToSuggestions()
        {
            var overallRect = RectangleF.Empty;
            for (var index = 0; index < _model.SuggestionLists.Count; index++)
            {
                var firstElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, 0, false);
                var firstRect = _layout.GetTargetRect(firstElement);
                overallRect = RectangleF.Union(overallRect, firstRect);

                var list = _model.SuggestionLists[index];
                var count = list.Count();
                if (1 < count)
                {
                    var lastElement = new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, list.Count() - 1, false);
                    var lastRect = _layout.GetTargetRect(lastElement);
                    overallRect = RectangleF.Union(overallRect, lastRect);
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

        private void AddRectangle(RectangleF overallRect, Action action)
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
