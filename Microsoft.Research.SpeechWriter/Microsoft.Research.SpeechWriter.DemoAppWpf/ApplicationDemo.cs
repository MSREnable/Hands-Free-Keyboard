using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.DemoAppWpf
{
    internal class ApplicationDemo
    {
        private readonly MainWindow _host;

        private bool _demoMode;

        private bool _demoMovementAnimation;

        private ApplicationDemo(MainWindow host)
        {
            _host = host;
        }

        internal static ApplicationDemo Create(MainWindow host)
        {
            var demo = new ApplicationDemo(host);
            host.PreviewKeyDown += (s, e) =>
              {
                  if (demo.DoSpecialKey(e.Key))
                  {
                      e.Handled = true;
                  }
              };
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


        private bool DoSpecialKey(Key key)
        {
            var done = true;

            switch (key)
            {
                case Key.X:
                    OnClickKirk();
                    break;

                default:
                    done = false;
                    break;
            }

            return done;
        }
    }
}