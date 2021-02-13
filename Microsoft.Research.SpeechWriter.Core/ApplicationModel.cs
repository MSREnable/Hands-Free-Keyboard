using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

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

        private readonly ObservableCollection<ICommand> _closingItems = new ObservableCollection<ICommand>();

        private readonly ObservableCollection<IEnumerable<ICommand>> _nextSuggestions = new ObservableCollection<IEnumerable<ICommand>>();
        private readonly ObservableCollection<ICommand> _suggestionInterstitials = new ObservableCollection<ICommand>();

        private readonly ObservableCollection<ICommand> _combined = new ObservableCollection<ICommand>();

        private int _previousWordsLengthNotified;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ApplicationModel(IWriterEnvironment environment)
        {
            Environment = environment;

            _wordSource = new WordVocabularySource(this);

            TailItems = new ReadOnlyObservableCollection<ICommand>(_closingItems);
            _closingItems.Add(new TailStopItem(_wordSource));

            SuggestionLists = new ReadOnlyObservableCollection<IEnumerable<ICommand>>(_nextSuggestions);
            SuggestionInterstitials = new ReadOnlyObservableCollection<ICommand>(_suggestionInterstitials);

            HeadItems = new ReadOnlyObservableCollection<ICommand>(_combined);
            LoadCombined(null, null);
            ((INotifyCollectionChanged)SelectedItems).CollectionChanged += LoadCombined;
            ((INotifyCollectionChanged)RunOnSuggestions).CollectionChanged += LoadCombined;

            Source = _wordSource;
            _wordSource.ResetSuggestionsView();

            void LoadCombined(object sender, NotifyCollectionChangedEventArgs e)
            {
                _combined.Clear();
                foreach (var item in SelectedItems)
                {
                    _combined.Add(item);
                }
                foreach (var item in RunOnSuggestions)
                {
                    _combined.Add(item);
                }
            }
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
        /// The currently selected items.
        /// </summary>
        public ReadOnlyObservableCollection<ICommand> SelectedItems => _wordSource.SelectedItems;

        /// <summary>
        /// The open items that can be closed.
        /// </summary>
        public ReadOnlyObservableCollection<ICommand> TailItems { get; }

        /// <summary>
        /// Next word suggestion list. (Several individual words, one of which may be used next.)
        /// </summary>
        public ReadOnlyObservableCollection<IEnumerable<ICommand>> SuggestionLists { get; }

        /// <summary>
        /// Items between suggestion lists.
        /// </summary>
        public ReadOnlyObservableCollection<ICommand> SuggestionInterstitials { get; }

        /// <summary>
        /// Following words suggestions. (Sequence of several words that may all be used next.)
        /// </summary>
        internal ReadOnlyObservableCollection<ICommand> RunOnSuggestions => _wordSource.RunOnSuggestions;

        /// <summary>
        /// Combined lists.
        /// </summary>
        public ReadOnlyObservableCollection<ICommand> HeadItems { get; }

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
                    words.Add(HeadItems[i].ToString());
                }
                Debug.Assert(HeadItems.Count == words.Count + 2);
                Debug.Assert(HeadItems[HeadItems.Count - 1] is GhostStopItem);

                nextPreviousWordsLength = 0;
            }
            else
            {
                for (var i = 1; i < HeadItems.Count && HeadItems[i] is HeadWordItem; i++)
                {
                    words.Add(HeadItems[i].ToString());
                }

                nextPreviousWordsLength = words.Count;
            }


            var e = new ApplicationModelUpdateEventArgs(words, _previousWordsLengthNotified, isComplete);
            _previousWordsLengthNotified = nextPreviousWordsLength;

            _applicationModelUpdate?.Invoke(this, e);
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
                    var interstitialItem = Source.CreatePriorInterstitial(index);
                    _suggestionInterstitials.Add(interstitialItem ?? new InterstitialNonItem());
                }
                else
                {
                    if (previousIndex != -1)
                    {
                        EmitInterstitial(previousIndex + 1, index);
                    }
                    else if (_lowerBound < index)
                    {
                        EmitInterstitial(_lowerBound, index);
                    }
                    else
                    {
                        var interstitialItem = new InterstitialGapItem(this, Source, 0, Math.Max(_lowerBound, maxItemCount));
                        _suggestionInterstitials.Add(interstitialItem);
                    }
                }

                var suggestionList = Source.CreateSuggestionList(index);
                _nextSuggestions.Add(suggestionList);

                previousIndex = index;
            }

            if (previousIndex + 1 == Source.Count)
            {
                var interstitial = Source.CreatePriorInterstitial(previousIndex + 1);
                _suggestionInterstitials.Add(interstitial);
            }
            else if (previousIndex + 1 < _upperLimit)
            {
                EmitInterstitial(previousIndex + 1, _upperLimit);
            }
            else
            {
                EmitInterstitial(Math.Min(previousIndex + 1, Source.Count - maxItemCount), Source.Count);
            }

            RaiseApplicationModelUpdateEvent(isComplete);

            void EmitInterstitial(int min, int lim)
            {
                var underrun = Math.Max(0, maxItemCount - (lim - min));
                var adjustedMin = Math.Max(0, min - underrun / 2);
                var adjustedLim = Math.Max(adjustedMin + maxItemCount, lim);

                var overrun = adjustedLim - Source.Count;
                if (0 < overrun)
                {
                    adjustedMin -= overrun;
                    adjustedLim -= overrun;
                }

                var interstitial = new InterstitialGapItem(this, Source, adjustedMin, adjustedLim);
                _suggestionInterstitials.Add(interstitial);
            }
        }
    }
}
