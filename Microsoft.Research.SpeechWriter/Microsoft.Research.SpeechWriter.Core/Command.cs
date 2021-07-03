using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Base class for command items.
    /// </summary>
    public abstract class Command<TSource> : ITile
        where TSource : VocabularySource
    {
        private readonly ITile _predecessor;
        private readonly ApplicationModel _model;
        private readonly TSource _source;

        internal Command(ITile predecessor, TSource source)
        {
            Debug.Assert(source != null);

            _predecessor = predecessor;
            _model = source.Model;
            _source = source;
        }

        internal Command(ITile predecessor, ApplicationModel model)
        {
            _predecessor = predecessor;
            _model = model;
        }

        /// <summary>
        /// The tile language.
        /// </summary>
        public virtual CultureInfo Culture => _predecessor.Culture;

        /// <summary>
        /// Tile that preceeds this.
        /// </summary>
        public ITile Predecessor => _predecessor;

        /// <summary>
        /// Is this item changed by conversion to uppercase or lowercase?
        /// </summary>
        public virtual bool IsCased => false;

        /// <summary>
        /// Does this item follow an item with IsCase true?
        /// </summary>
        public virtual bool IsCasedSuccessor => _predecessor.IsCasedSuccessor || _predecessor.IsCased;

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public abstract string Content { get; }

        /// <summary>
        /// The formatted content of the tile.
        /// </summary>
        public virtual string FormattedContent => Content;

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
            _model.Trace(this);

            Execute(_source);
        }

        internal virtual void TraceContent(XmlWriter writer)
        {
        }

        internal abstract void Execute(TSource source);

        /// <summary>
        /// Standard override.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString() => $"{GetType().Name} - {Content}";
    }
}
