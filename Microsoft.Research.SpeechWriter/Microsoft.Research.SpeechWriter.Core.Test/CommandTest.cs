using NUnit.Framework;
using System;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    public class CommandTest
    {
        private class TestCommand : Command<WordVocabularySource>
        {
            internal bool _called;

            internal TestCommand(ApplicationModel model)
                : base(null, model)
            {
            }

            /// <summary>
            /// The basic content of the tile.
            /// </summary>
            public override string Content => "Wibble";

            internal override void Execute(WordVocabularySource source)
            {
                Assert.IsNull(source);

                _called = true;
            }
        }

        [Test]
        public void CoverNonFunctions()
        {
            var model = new ApplicationModel();
            ITile command = new TestCommand(model);

            OnCanExecuteChanged(this, EventArgs.Empty);
            command.CanExecuteChanged += OnCanExecuteChanged;
            command.CanExecuteChanged -= OnCanExecuteChanged;

            Assert.IsTrue(command.CanExecute(null));

            Assert.IsFalse(((TestCommand)command)._called);
            command.Execute(null);
            Assert.IsTrue(((TestCommand)command)._called);

            Assert.AreEqual("Wibble", command.Content);
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            Assert.AreSame(this, sender);
            Assert.AreSame(EventArgs.Empty, e);
        }
    }
}
