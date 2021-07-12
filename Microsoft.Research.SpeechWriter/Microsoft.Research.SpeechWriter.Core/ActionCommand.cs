using System;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class ActionCommand : ICommand
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
}
