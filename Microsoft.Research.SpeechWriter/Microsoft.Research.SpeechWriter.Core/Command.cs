using System;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Base class for command items.
    /// </summary>
    public abstract class Command<TSource> : ITile
        where TSource : VocabularySource
    {
        private readonly TSource _source;

        internal Command(TSource source)
        {
            _source = source;
        }

        /// <summary>
        /// The source.
        /// </summary>
        protected TSource Source => _source;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter) => true;

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            Execute(_source);
        }

        internal abstract void Execute(TSource source);
    }
}
