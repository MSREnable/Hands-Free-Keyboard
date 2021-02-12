using Microsoft.Research.SpeechWriter.Core.Items;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// A source containing an ordered list of vocabulary items.
    /// </summary>
    public abstract class VocabularySource
    {
        private readonly ApplicationModel _model;

        internal VocabularySource(ApplicationModel model)
        {
            _model = model;
        }

        /// <summary>
        /// The environment.
        /// </summary>
        protected IWriterEnvironment Environment => _model.Environment;

        internal abstract int Count { get; }

        internal abstract IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count);

        /// <summary>
        /// Get the suggestion list stemming from the indexed item.
        /// </summary>
        /// <param name="index">The index within the source.</param>
        /// <returns>The suggestions list.</returns>
        internal abstract IEnumerable<ICommand> CreateSuggestionList(int index);

        /// <summary>
        /// Get an item that comes before the specified item and the immediately preceeding item.
        /// </summary>
        /// <param name="index">Index of item, 0 &lt;= index &lt;= Count. 0 for item before first,
        /// Count for item after last.
        /// </param>
        /// <returns></returns>
        internal virtual ICommand CreatePriorInterstitial(int index) => null;

        internal void ResetSuggestionsView()
        {
            _model.SetSuggestionsView(_model.Source, 0, _model.Source.Count);
        }

        internal void SetSuggestionsView()
        {
            _model.SetSuggestionsView(this, 0, Count);
        }
    }
}
