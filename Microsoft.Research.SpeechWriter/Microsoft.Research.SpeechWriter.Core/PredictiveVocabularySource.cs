using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// A source containing an ordered list of vocabulary items.
    /// </summary>
    public abstract class PredictiveVocabularySource : VocabularySource
    {
        private readonly TokenPredictor _persistantPredictor;

        private readonly TokenPredictor _temporaryPredictor;

        internal PredictiveVocabularySource(ApplicationModel model, int predictorWidth)
            : base(model)
        {
            _persistantPredictor = new TokenPredictor(predictorWidth);
            _temporaryPredictor = new TokenPredictor(predictorWidth);
        }

        /// <summary>
        /// 
        /// </summary>
        protected TokenPredictor PersistantPredictor => _persistantPredictor;

        /// <summary>
        /// 
        /// </summary>
        protected TokenPredictor TemporaryPredictor => _temporaryPredictor;

        internal abstract int[] GetContext();

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
            var context = GetContext();

            var result = PersistantPredictor.GetTopIndices(this, context, minIndex, limIndex, count);
            return result;
        }

        /// <summary>
        /// Get token values in an order that will roughly correspond to their likelyhood without any context.
        /// </summary>
        /// <returns>An enumeration of integer tokens.</returns>
        internal abstract IEnumerable<int> GetTokens();

        internal abstract ICommand GetIndexItem(int index);

        /// <summary>
        /// Get the item sequence that corresponds to the given index.
        /// </summary>
        /// <param name="index">The index within the source.</param>
        /// 
        /// <returns>The vocabulary item corresponding to the index.</returns>
        internal override IEnumerable<ICommand> CreateSuggestionList(int index)
        {
            var item = GetIndexItem(index);

            yield return item;

            var token = GetIndexToken(index);
            if (token != 0)
            {
                var context = GetContext();
                var extraContext = new List<int>(context);
                extraContext.Add(token);

                var sequence = new List<int>();
                sequence.Add(token);

                var more = true;
                for (var extras = 0; more && extras < 3; extras++)
                {
                    var extraToken = GetTopToken(extraContext.ToArray());
                    if (extraToken != -1)
                    {
                        extraContext.Add(extraToken);
                        sequence.Add(extraToken);

                        var extraItem = GetSequenceItem(sequence);
                        yield return extraItem;

                        more = extraToken != 0;
                    }
                    else
                    {
                        more = false;
                    }
                }
            }
        }

        /// <summary>
        /// Get follow in compound item.
        /// </summary>
        /// <param name="tokenSequence">The sequence of tokens.</param>
        /// <returns></returns>
        internal abstract ICommand GetSequenceItem(IEnumerable<int> tokenSequence);

        /// <summary>
        /// Add the next step of the sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="increment"></param>
        internal void AddSequence(IList<int> sequence, int increment)
        {
            PersistantPredictor.AddSequenceTail(sequence, increment);
            TemporaryPredictor.AddSequenceTail(sequence, increment);
        }
    }
}
