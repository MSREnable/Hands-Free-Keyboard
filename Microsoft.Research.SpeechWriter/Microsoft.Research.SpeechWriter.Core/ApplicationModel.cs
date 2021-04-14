using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Exported application model.
    /// </summary>
    public class ApplicationModel
    {
        private readonly WordVocabularySource _wordSource;

        private int _lowerBound;
        private int _upperLimit;

        private readonly ObservableCollection<ITile> _closingItems = new ObservableCollection<ITile>();

        private readonly ObservableCollection<IEnumerable<ITile>> _nextSuggestions = new ObservableCollection<IEnumerable<ITile>>();
        private readonly ObservableCollection<ITile> _suggestionInterstitials = new ObservableCollection<ITile>();

        private int _previousWordsLengthNotified;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ApplicationModel(IWriterEnvironment environment)
        {
            Environment = environment;

            _wordSource = new WordVocabularySource(this);

            TailItems = new ReadOnlyObservableCollection<ITile>(_closingItems);
            _closingItems.Add(new TailStopItem(null, _wordSource));

            SuggestionLists = new ReadOnlyObservableCollection<IEnumerable<ITile>>(_nextSuggestions);
            SuggestionInterstitials = new ReadOnlyObservableCollection<ITile>(_suggestionInterstitials);

            Source = _wordSource;
            _wordSource.ResetSuggestionsView();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ApplicationModel()
            : this(DefaultWriterEnvironment.Instance)
        {
        }

        /// <summary>
        /// The environment.
        /// </summary>
        public IWriterEnvironment Environment { get; }

        /// <summary>
        /// The maximum number of next word suggestions to make.
        /// </summary>
        public int MaxNextSuggestionsCount
        {
            get { return _maxNextSuggestinosCount; }
            set
            {
                if (_maxNextSuggestinosCount != value)
                {
                    _maxNextSuggestinosCount = value;
                    SetSuggestionsView(Source, _lowerBound, _upperLimit, false);
                }
            }
        }
        private int _maxNextSuggestinosCount = 9;

        internal VocabularySource Source { get; private set; }

        /// <summary>
        /// The open items that can be closed.
        /// </summary>
        public ReadOnlyObservableCollection<ITile> TailItems { get; }

        /// <summary>
        /// Next word suggestion list. (Several individual words, one of which may be used next.)
        /// </summary>
        public ReadOnlyObservableCollection<IEnumerable<ITile>> SuggestionLists { get; }

        /// <summary>
        /// Items between suggestion lists.
        /// </summary>
        public ReadOnlyObservableCollection<ITile> SuggestionInterstitials { get; }

        /// <summary>
        /// Combined lists.
        /// </summary>
        public ReadOnlyObservableCollection<ITile> HeadItems => _wordSource.HeadItems;

        /// <summary>
        /// The last selected tile.
        /// </summary>
        internal ITile LastTile => _wordSource.LastTile;

        /// <summary>
        /// Event occurring afer every model update.
        /// </summary>
        public event EventHandler<ApplicationModelUpdateEventArgs> ApplicationModelUpdate
        {
            add { _applicationModelUpdate += value; }
            remove { _applicationModelUpdate -= value; }
        }
        private event EventHandler<ApplicationModelUpdateEventArgs> _applicationModelUpdate;

        private void RaiseApplicationModelUpdateEvent(bool isComplete)
        {
            var words = new List<string>();
            int nextPreviousWordsLength;

            Debug.Assert(HeadItems[0] is HeadStartItem);
            if (isComplete)
            {
                for (var i = 1; i < HeadItems.Count && HeadItems[i] is GhostWordItem; i++)
                {
                    words.Add(HeadItems[i].Content);
                }
                Debug.Assert(HeadItems.Count == words.Count + 2);
                Debug.Assert(HeadItems[HeadItems.Count - 1] is GhostStopItem);

                nextPreviousWordsLength = 0;
            }
            else
            {
                for (var i = 1; i < HeadItems.Count && HeadItems[i] is HeadWordItem; i++)
                {
                    words.Add(HeadItems[i].Content);
                }

                nextPreviousWordsLength = words.Count;
            }


            var e = new ApplicationModelUpdateEventArgs(words, _previousWordsLengthNotified, isComplete);
            _previousWordsLengthNotified = nextPreviousWordsLength;

            _applicationModelUpdate?.Invoke(this, e);
        }

        /// <summary>
        /// Load utterances.
        /// </summary>
        /// <returns></returns>
        public async Task LoadUtterancesAsync()
        {
            await _wordSource.LoadUtterancesAsync();
        }

        internal void SetSuggestionsView(VocabularySource source, int lowerBound, int upperLimit, bool isComplete)
        {
            Debug.Assert(!(source is SpellingVocabularySource));

            Source = source;
            _lowerBound = lowerBound;
            _upperLimit = upperLimit;

            ResetSuggestionsView(isComplete);
        }

        private void ResetSuggestionsView(bool isComplete)
        {
            var maxItemCount = Math.Min(Source.Count, MaxNextSuggestionsCount - 1);
            var rankedIndices = Source.GetTopIndices(_lowerBound, _upperLimit, maxItemCount);

            var sortedIndices = new SortedSet<int>(rankedIndices);

            Debug.Assert(rankedIndices.Count() == sortedIndices.Count);

            _suggestionInterstitials.Clear();
            _nextSuggestions.Clear();

            var previousIndex = -1;
            foreach (var index in sortedIndices)
            {
                if (previousIndex + 1 == index)
                {
                    // Item contiguous with previous one, so allow it to dictate its interstitial.
                    var interstitialItem = Source.CreatePriorInterstitial(index);
                    _suggestionInterstitials.Add(interstitialItem ?? new InterstitialNonItem());
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
                        var interstitialItem = new InterstitialGapItem(LastTile, this, Source, 0, Math.Max(_lowerBound, maxItemCount));
                        _suggestionInterstitials.Add(interstitialItem);
                    }
                }

                var suggestionList = Source.CreateSuggestionList(index);
                _nextSuggestions.Add(suggestionList);

                previousIndex = index;
            }

            if (previousIndex + 1 == Source.Count)
            {
                // Last item in source, so allow it to dictate the final interstitial.
                var interstitial = Source.CreatePriorInterstitial(previousIndex + 1);
                _suggestionInterstitials.Add(interstitial);
            }
            else if (previousIndex + 1 < _upperLimit)
            {
                // Last item of bounded items, but not necessary last in source.
                EmitInterstitial(previousIndex + 1, _upperLimit);
            }
            else
            {
                Debug.Assert(_upperLimit < Source.Count);

                // Last item was last of bounded items, but not of source, so emit interstitial to end.
                EmitInterstitial(Math.Min(previousIndex + 1, Source.Count - maxItemCount), Source.Count);
            }

            RaiseApplicationModelUpdateEvent(isComplete);

            void EmitInterstitial(int min, int lim)
            {
                var effectiveMin = min;
                var effectiveLim = lim;

                if (min != 0 && Source.CreatePriorInterstitial(min) != null)
                {
                    // If we're top is going to replace custom interstitial, extend top by one item.
                    effectiveMin--;
                }

                if (lim != Source.Count && Source.CreatePriorInterstitial(lim) != null)
                {
                    // If we're at the bottom and going to replace custom interstitial, extend bottom by one item.
                    effectiveLim++;
                }

                var underrun = Math.Max(0, maxItemCount - (effectiveLim - effectiveMin));
                var adjustedMin = Math.Max(0, effectiveMin - underrun / 2);
                var adjustedLim = Math.Max(adjustedMin + maxItemCount, effectiveLim);

                var overrun = adjustedLim - Source.Count;
                if (0 < overrun)
                {
                    adjustedMin -= overrun;
                    adjustedLim -= overrun;
                }

                var interstitial = new InterstitialGapItem(LastTile, this, Source, adjustedMin, adjustedLim);
                _suggestionInterstitials.Add(interstitial);
            }
        }
    }
}
