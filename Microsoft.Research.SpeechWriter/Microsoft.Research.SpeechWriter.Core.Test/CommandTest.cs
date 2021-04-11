using NUnit.Framework;
using System;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    public class CommandTest
    {
        private class TestCommand : Command<WordVocabularySource>
        {
            internal bool _called;

            internal TestCommand()
                : base(null, null)
            {
            }

            internal override void Execute(WordVocabularySource source)
            {
                Assert.IsNull(source);

                _called = true;
            }
        }

        [Test]
        public void CoverNonFunctions()
        {
            ICommand command = new TestCommand();

            OnCanExecuteChanged(this, EventArgs.Empty);
            command.CanExecuteChanged += OnCanExecuteChanged;
            command.CanExecuteChanged -= OnCanExecuteChanged;

            Assert.IsTrue(command.CanExecute(null));

            Assert.IsFalse(((TestCommand)command)._called);
            command.Execute(null);
            Assert.IsTrue(((TestCommand)command)._called);
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            Assert.AreSame(this, sender);
            Assert.AreSame(EventArgs.Empty, e);
        }
    }
}
