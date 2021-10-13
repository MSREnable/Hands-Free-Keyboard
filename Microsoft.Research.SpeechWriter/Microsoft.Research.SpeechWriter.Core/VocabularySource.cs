using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        internal abstract IReadOnlyList<ITile> CreateSuggestionList(int index);

        internal virtual ITile GetIndexItemForTrace(int index) => CreateSuggestionList(index).FirstOrDefault();

        /// <summary>
        /// Get an item that comes before the specified item and the immediately preceeding item.
        /// </summary>
        /// <param name="index">Index of item, 0 &lt;= index &lt;= Count. 0 for item before first,
        /// Count for item after last.
        /// </param>
        /// <returns></returns>
        internal virtual ITile CreatePriorInterstitial(int index) => null;

        internal void SetSuggestionsView(int lowerBound, int upperLimit, bool isComplete)
        {
            UpdateSuggestionsView(lowerBound, upperLimit, isComplete);
        }

        internal void SetSuggestionsView()
        {
            SetSuggestionsView(0, Count, false);
        }

        internal void SetSuggestionsViewComplete()
        {
            SetSuggestionsView(0, Count, true);
        }

        private bool AreAdjacentIndices(int lowerIndex, int upperIndex)
        {
            var adjustedLowerIndex = lowerIndex;
            while (adjustedLowerIndex < upperIndex && !Filter.IsIndexVisible(adjustedLowerIndex))
            {
                adjustedLowerIndex++;
            }

            return adjustedLowerIndex == upperIndex;
        }

        private SortedList<int, IReadOnlyList<ITile>> CreateSuggestionLists(int lowerBound, int upperBound, int maxItemCount)
        {
            var value = new SortedList<int, IReadOnlyList<ITile>>();

            var topIndices = GetTopIndices(lowerBound, upperBound, maxItemCount);
            foreach (var index in topIndices)
            {
                var tiles = CreateSuggestionList(index);
                var list = new List<ITile>(tiles);
                value.Add(index, list);
            }

            return value;
        }

        private void UpdateSuggestionsView(int _lowerBound, int _upperLimit, bool isComplete)
        {
            var maxItemCount = Math.Min(Count, Model.MaxNextSuggestionsCount - 1);
            var suggestionLists = CreateSuggestionLists(_lowerBound, _upperLimit, maxItemCount);

            var suggestions = new List<IReadOnlyList<ITile>>();
            var suggestionInterstitials = new List<ITile>();

            var previousIndex = -1;
            foreach (var pair in suggestionLists)
            {
                var index = pair.Key;

                if (AreAdjacentIndices(previousIndex + 1, index))
                {
                    // Item contiguous with previous one, so allow it to dictate its interstitial.
                    EmitPriorInterstitial(index);
                }
                else
                {
                    if (previousIndex != -1)
                    {
                        // Not the first item, so items that are displayed adjacent to one another, but are not actually contiguous.
                        EmitInterstitial(previousIndex + 1, index);
                    }
                    else if (_lowerBound < index)
                    {
                        // First item, but within the bounds of the items being displayed, so need interstitial to start of bounds.
                        EmitInterstitial(_lowerBound, index);
                    }
                    else
                    {
                        Debug.Assert(index <= _lowerBound);

                        // First item of bounded area, so emit interstitial from start to this item.
                        var interstitialItem = new InterstitialGapItem(Model.LastTile, Model, this, 0, Math.Max(_lowerBound, maxItemCount));
                        suggestionInterstitials.Add(interstitialItem);
                    }
                }

                suggestions.Add(pair.Value);

                previousIndex = index;
            }

            if (AreAdjacentIndices(previousIndex + 1, Count))
            {
                // Last item in source, so allow it to dictate the final interstitial.
                EmitPriorInterstitial(Count);
            }
            else if (previousIndex + 1 < _upperLimit)
            {
                // Last item of bounded items, but not necessary last in source.
                EmitInterstitial(previousIndex + 1, _upperLimit);
            }
            else
            {
                Debug.Assert(_upperLimit < Count);

                // Last item was last of bounded items, but not of source, so emit interstitial to end.
                EmitInterstitial(Math.Min(previousIndex + 1, Count - maxItemCount), Count);
            }

            Model.UpdateSuggestions(this, _lowerBound, _upperLimit, isComplete, suggestions, suggestionInterstitials);

            void EmitPriorInterstitial(int index)
            {
                var interstitialItem = CreatePriorInterstitial(index);
                var actualItem = interstitialItem ?? new InterstitialNonItem(Model);
                suggestionInterstitials.Add(actualItem);
            }

            void EmitInterstitial(int min, int lim)
            {
                var effectiveMin = min;
                var effectiveLim = lim;

                if (min != 0 && CreatePriorInterstitial(min) != null)
                {
                    // If we're top is going to replace custom interstitial, extend top by one item.
                    effectiveMin--;
                }

                if (lim != Count && CreatePriorInterstitial(lim) != null)
                {
                    // If we're at the bottom and going to replace custom interstitial, extend bottom by one item.
                    effectiveLim++;
                }

                var underrun = Math.Max(0, maxItemCount - (effectiveLim - effectiveMin));
                var adjustedMin = Math.Max(0, effectiveMin - underrun / 2);
                var adjustedLim = Math.Max(adjustedMin + maxItemCount, effectiveLim);

                var overrun = adjustedLim - Count;
                if (0 < overrun)
                {
                    adjustedMin -= overrun;
                    adjustedLim -= overrun;
                }

                var interstitial = new InterstitialGapItem(Model.LastTile, Model, this, adjustedMin, adjustedLim);
                suggestionInterstitials.Add(interstitial);
            }
        }
    }
}
