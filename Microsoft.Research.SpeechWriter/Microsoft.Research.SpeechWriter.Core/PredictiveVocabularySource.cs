using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// A source containing an ordered list of vocabulary items.
    /// </summary>
    public abstract class PredictiveVocabularySource<TItem> : VocabularySource
        where TItem : ISuggestionItem
    {
        private readonly TokenPredictor _persistantPredictor;

        private readonly TokenPredictor _deltaPredictor;

        internal PredictiveVocabularySource(ApplicationModel model, int predictorWidth)
            : base(model)
        {
            _persistantPredictor = new TokenPredictor(predictorWidth);
            _deltaPredictor = new TokenPredictor(predictorWidth);
        }

        /// <summary>
        /// Current active predictor database.
        /// </summary>
        protected TokenPredictor PersistantPredictor => _persistantPredictor;

        /// <summary>
        /// Predictor database containing only recently added and perhaps speculitve additions.
        /// </summary>
        protected TokenPredictor DeltaPredictor => _deltaPredictor;

        internal int[] Context { get; private set; }

        internal sealed override ITileFilter Filter => TokenFilter;

        internal abstract ITokenTileFilter TokenFilter { get; }

        internal void SetContext(int[] context)
        {
            Context = context;

            ResetTileFilter();
        }

        internal void SetContext(List<int> context)
        {
            SetContext(context.ToArray());
        }

        internal virtual void ResetTileFilter()
        {

        }

        /// <summary>
        /// Get the non-zero token for at a given index within the source.
        /// </summary>
        /// <param name="index">Index of item, such that 0 &gt;= index &gt; Count.</param>
        /// <returns>The token at the give index</returns>
        internal abstract int GetIndexToken(int index);

        /// <summary>
        /// Get the position index of the given token with the source.
        /// </summary>
        /// <param name="token">The non-zero token.</param>
        /// <returns>The corresponding index position.</returns>
        internal abstract int GetTokenIndex(int token);

        /// <summary>
        /// Gets the top ranked token in the given context.
        /// </summary>
        /// <param name="context">The tokens before item to be suggested.</param>
        internal int GetTopToken(int[] context)
        {
            var result = PersistantPredictor.GetTopToken(context);
            return result;
        }

        internal override IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count)
        {
            var result = PersistantPredictor.GetTopIndices(this, TokenFilter, Context, minIndex, limIndex, count);
            return result;
        }

        /// <summary>
        /// Get token values in an order that will roughly correspond to their likelyhood without any context.
        /// </summary>
        /// <returns>An enumeration of integer tokens.</returns>
        internal abstract IEnumerable<int> GetTokens();

        internal abstract TItem GetIndexItem(int index);

        /// <summary>
        /// Get the item sequence that corresponds to the given index.
        /// </summary>
        /// <param name="index">The index within the source.</param>
        /// 
        /// <returns>The vocabulary item corresponding to the index.</returns>
        internal override IReadOnlyList<ITile> CreateSuggestionList(int index)
        {
            var value = new List<ITile>();

            ISuggestionItem item = GetIndexItem(index);

            value.Add(item);

            var token = GetIndexToken(index);
            if (token != 0)
            {
                var extraContext = new List<int>(Context) { token };

                var more = true;
                for (var extras = 0; more && extras < 3; extras++)
                {
                    var extraToken = GetTopToken(extraContext.ToArray());
                    if (extraToken != -1)
                    {
                        item = item.GetNextItem(extraToken);
                        value.Add(item);

                        extraContext.Add(extraToken);
                        more = extraToken != 0;
                    }
                    else
                    {
                        more = false;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Add the next step of the sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="increment"></param>
        internal void AddSequenceTail(IReadOnlyList<int> sequence, int increment)
        {
            PersistantPredictor.AddSequenceTail(sequence, increment);
            DeltaPredictor.AddSequenceTail(sequence, increment);
        }
    }
}
