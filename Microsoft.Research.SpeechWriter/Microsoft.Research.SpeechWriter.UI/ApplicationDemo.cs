using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using Microsoft.Research.SpeechWriter.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class ApplicationDemo
    {
        class ActionCommand : ICommand
        {
            private readonly Action _action;

            internal ActionCommand(Action action) => _action = action;

            event EventHandler ICommand.CanExecuteChanged
            {
                add
                {
                }

                remove
                {
                }
            }

            bool ICommand.CanExecute(object parameter) => true;

            void ICommand.Execute(object parameter) => _action();
        }

        private readonly IApplicationHost _host;

        private bool _demoMode;

        private bool _demoMovementAnimation;

        private List<TileSequence> _tutorScript;

        private ApplicationDemo(IApplicationHost host)
        {
            _host = host;
        }

        public ICommand Restart => new ActionCommand(OnRestart);
        public ICommand ClickKirk => new ActionCommand(OnClickKirk);
        public ICommand ClickPicard => new ActionCommand(OnClickPicard);
        public ICommand Paste => new ActionCommand(OnPaste);
        public ICommand ClickTutor => new ActionCommand(OnClickTutor);
        public ICommand ClickQuick => new ActionCommand(OnClickQuick);
        public ICommand ClickReset => new ActionCommand(OnClickReset);
        public ICommand TimingChange => new ActionCommand(OnTimingChange);
        public ICommand ShowLogging => new ActionCommand(OnShowLogging);
        public ICommand ShowTestCard => new ActionCommand(OnShowTestCard);
        public ICommand ImportClipboard => new ActionCommand(OnImportClipboard);

        public static ApplicationDemo Create(IApplicationHost host)
        {
            var demo = new ApplicationDemo(host);
            return demo;
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
                    _host.ShowTargetOutline();

                    var sequence = sentences[i];

                    bool done;
                    do
                    {
                        var action = ApplicationRobot.GetNextCompletionAction(_host.Model, sequence);

                        _host.SetupStoryboardForAction(action);

                        if (_demoMovementAnimation)
                        {
                            await _host.PlayMoveRectangleAsync();
                        }

                        var reaction = ApplicationRobot.GetNextCompletionAction(_host.Model, sequence);
                        if (action.Target == reaction.Target &&
                            action.Index == reaction.Index &&
                            action.SubIndex == reaction.SubIndex)
                        {
                            action.ExecuteItem(_host.Model);
                        }
                        await Task.Delay(TimeSpan.FromSeconds(0.1));

                        done = action.IsComplete;
                    }
                    while (_demoMode && !done);
                }

                _demoMode = false;
                _host.HideTargetOutline();
            }
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

        private void OnRestart()
        {
            _host.Restart(true);
        }

        private void OnClickKirk()
        {
            ShowDemo("space",
                "the final frontier",
                "these are the voyages of the starship Enterprise",
                "its five year mission",
                "to explore strange new worlds",
                "to seek out new life",
                "and new civilizations",
                "to boldly go where no man has gone before");
        }

        private void OnClickPicard()
        {
            ShowDemo("these are the voyages of the starship Enterprise",
                "its continuing mission",
                "to explore strange new worlds",
                "to seek out new life",
                "and new civilizations",
                "to boldly go where no one has gone before");
        }

        private async Task<List<TileSequence>> GetClipboardContentAsync()
        {
            var script = new List<TileSequence>();

            var text = await _host.GetClipboardStringAsync();

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

        private async void OnPaste()
        {
            var script = await GetClipboardContentAsync();

            if (script.Count != 0)
            {
                ShowDemo(script);
            }
        }

        private async Task ShowNextTutorStepAsync()
        {
            await Task.Delay(50);
            var words = _tutorScript[0];
            var action = ApplicationRobot.GetNextCompletionAction(_host.Model, words);
            _host.SetupStoryboardForAction(action);
            _ = _host.PlayTutorMoveStoryboardAsync();
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
                        _host.Model.ApplicationModelUpdate -= OnApplicationTutorReady;

                        _host.HideTargetOutline();
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
                _host.Model.ApplicationModelUpdate -= OnApplicationTutorReady;
            }
        }

        private async void OnClickTutor()
        {
            _demoMode = false;

            var script = await GetClipboardContentAsync();
            if (script.Count != 0)
            {
                _tutorScript = script;
                _host.ShowTargetOutline();

                _host.Model.ApplicationModelUpdate += OnApplicationTutorReady;
                _ = ShowNextTutorStepAsync();
            }
        }

        private void OnClickQuick()
        {
            _demoMovementAnimation = false;
        }

        private void OnClickReset()
        {
            _host.Restart(false);
        }

        private void OnTimingChange()
        {
            if (_host.MoveRectangeSeekTimeSpan.TotalSeconds == 1)
            {
                _host.MoveRectangeSeekTimeSpan = TimeSpan.FromSeconds(0.1);
                _host.MoveRectangeSettleTimeSpan = TimeSpan.FromSeconds(0.5);
            }
            else
            {
                _host.MoveRectangeSeekTimeSpan = TimeSpan.FromSeconds(1);
                _host.MoveRectangeSettleTimeSpan = TimeSpan.FromSeconds(1.25);
            }
        }

        private void OnShowLogging()
        {
            _host.ShowLogging();
        }

        private void OnShowTestCard()
        {
            _host.Model.ShowTestCard();
        }

        private async void OnImportClipboard()
        {
            var data = await GetClipboardContentAsync();
            foreach (var sequence in data)
            {
                await _host.Model.SaveUtteranceAsync(sequence, true);
            }
            _host.Restart(true);
        }

        public bool DoSpecialKey(object key)
        {
            var done = true;

            var keyString = key.ToString();
            switch (keyString)
            {
                case "F5": OnRestart(); break;

                case "X": OnClickKirk(); break;

                case "C": OnClickPicard(); break;

                case "V": OnPaste(); break;

                case "T": OnClickTutor(); break;

                case "Q": OnClickQuick(); break;

                case "R": OnClickReset(); break;

                case "S": OnTimingChange(); break;

                case "L": OnShowLogging(); break;

                case "P": OnShowTestCard(); break;

                case "I": OnImportClipboard(); break;

                default:
                    done = false;
                    break;
            }

            return done;
        }
    }
}