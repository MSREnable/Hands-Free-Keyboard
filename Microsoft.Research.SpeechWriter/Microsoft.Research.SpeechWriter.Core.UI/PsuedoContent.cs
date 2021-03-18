using System;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class PsuedoContent : ICommand
    {
        private readonly string _content;

        public PsuedoContent(string content)
        {
            _content = content;
        }

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

        void ICommand.Execute(object parameter)
        {
        }

        public override string ToString() => _content;
    }
}
