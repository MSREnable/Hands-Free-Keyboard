using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// A source containing an ordered list of vocabulary items.
    /// </summary>
    public abstract class VocabularySource
    {
        internal VocabularySource(ApplicationModel model)
        {
            Model = model;
        }

        internal ApplicationModel Model { get; }

        /// <summary>
        /// The environment.
        /// </summary>
        protected IWriterEnvironment Environment => Model.Environment;

        internal abstract int Count { get; }

        internal virtual ITileFilter Filter => DefaultTileFilter.Instance;

        internal abstract IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count);

        /// <summary>
        /// Get the suggestion list stemming from the indexed item.
        /// </summary>
        /// <param name="index">The index within the source.</param>
        /// <returns>The suggestions list.</returns>
        internal abstract IEnumerable<ITile> CreateSuggestionList(int index);

        internal virtual ITile GetIndexItemForTrace(int index) => CreateSuggestionList(index).FirstOrDefault();

        /// <summary>
        /// Get an item that comes before the specified item and the immediately preceeding item.
        /// </summary>
        /// <param name="index">Index of item, 0 &lt;= index &lt;= Count. 0 for item before first,
        /// Count for item after last.
        /// </param>
        /// <returns></returns>
        internal virtual ITile CreatePriorInterstitial(int index) => null;

        internal void ResetSuggestionsView()
        {
            Model.SetSuggestionsView(Model.Source, 0, Model.Source.Count, false);
        }

        internal void SetSuggestionsView()
        {
            Model.SetSuggestionsView(this, 0, Count, false);
        }

        internal void SetSuggestionsViewComplete()
        {
            Model.SetSuggestionsView(this, 0, Count, true);
        }
    }
}
