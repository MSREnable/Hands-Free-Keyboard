using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        private readonly ObservableCollection<IReadOnlyList<ITile>> _nextSuggestions = new ObservableCollection<IReadOnlyList<ITile>>();
        private readonly ObservableCollection<ITile> _suggestionInterstitials = new ObservableCollection<ITile>();

        private int _previousWordsLengthNotified;

        /// <summary>
        /// Has the utterance been terminated.
        /// </summary>
        public bool _isUtteranceComplete = true;

        /// <summary>
        /// When was the first activation of the utterance.
        /// </summary>
        public DateTimeOffset _utteranceStartTime;

        /// <summary>
        /// How long has this utterance been going.
        /// </summary>
        public TimeSpan _utteranceDuration;

        /// <summary>
        /// How many activations have been made in this utterance.
        /// </summary>
        public int _utteranceActivationCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ApplicationModel(IWriterEnvironment environment)
        {
            Environment = environment;

            _wordSource = new WordVocabularySource(this);

            SuggestionLists = new ReadOnlyObservableCollection<IReadOnlyList<ITile>>(_nextSuggestions);
            SuggestionInterstitials = new ReadOnlyObservableCollection<ITile>(_suggestionInterstitials);

            Source = _wordSource;
            _wordSource.SetSuggestionsView();
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
        /// The maximum number of next word suggestions to make plus one.
        /// </summary>
        public int DisplayRows
        {
            get => _displayRows;
            set
            {
                if (_displayRows != value)
                {
                    _displayRows = value;
                    RefreshSuggestionsView();
                }
            }
        }
        private int _displayRows = 9;

        public int DisplayColumns
        {
            get => _displayColumns;
            set
            {
                if (_displayColumns != value)
                {
                    _displayColumns = value;
                    RefreshSuggestionsView();
                }
            }
        }
        private int _displayColumns = 16;

        internal VocabularySource Source { get; private set; }

        /// <summary>
        /// The open items that can be closed.
        /// </summary>
        public ReadOnlyObservableCollection<ITile> TailItems => _wordSource.TailItems;

        /// <summary>
        /// Next word suggestion list. (Several individual words, one of which may be used next.)
        /// </summary>
        public ReadOnlyObservableCollection<IReadOnlyList<ITile>> SuggestionLists { get; }

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
        /// The index of the selected item.
        /// </summary>
        public int SelectedHeadIndex => _wordSource.SelectedIndex;

        /// <summary>
        /// Event occurring afer every model update.
        /// </summary>
        public event EventHandler<ApplicationModelUpdateEventArgs> ApplicationModelUpdate;

        private void RaiseApplicationModelUpdateEvent(bool isComplete)
        {
            var timestamp = Environment.GetTimestamp();
            if (_isUtteranceComplete)
            {
                _utteranceStartTime = timestamp;
                _utteranceDuration = TimeSpan.Zero;
                _utteranceActivationCount = 1;
            }
            else
            {
                _utteranceDuration = timestamp - _utteranceStartTime;
                _utteranceActivationCount++;
            }
            _isUtteranceComplete = isComplete;

            var words = new List<string>();
            int nextPreviousWordsLength;

            Debug.Assert(HeadItems[0] is HeadStartItem);
            if (isComplete)
            {
                for (var i = 1; i < HeadItems.Count && HeadItems[i] is GhostWordItem; i++)
                {
                    words.Add(HeadItems[i].FormattedContent);
                }
                Debug.Assert(HeadItems.Count == words.Count + 2);
                Debug.Assert(HeadItems[HeadItems.Count - 1] is GhostStopItem);

                nextPreviousWordsLength = 0;
            }
            else
            {
                for (var i = 1; i < HeadItems.Count && HeadItems[i] is HeadWordItem; i++)
                {
                    words.Add(HeadItems[i].FormattedContent);
                }

                nextPreviousWordsLength = words.Count;
            }

            var e = new ApplicationModelUpdateEventArgs(words, _previousWordsLengthNotified, isComplete,
                _utteranceStartTime, _utteranceDuration, _utteranceActivationCount);
            _previousWordsLengthNotified = nextPreviousWordsLength;

            ApplicationModelUpdate?.Invoke(this, e);
        }

        internal void UpdateSuggestions(VocabularySource source, int lowerBound, int upperLimit, bool isComplete, List<IReadOnlyList<ITile>> suggestions, List<ITile> suggestionInterstitials)
        {
            Source = source;
            _lowerBound = lowerBound;
            _upperLimit = upperLimit;

            _nextSuggestions.Clear();
            foreach (var suggestion in suggestions)
            {
                _nextSuggestions.Add(suggestion);
            }

            _suggestionInterstitials.Clear();
            foreach (var interstitial in suggestionInterstitials)
            {
                _suggestionInterstitials.Add(interstitial);
            }

            RaiseApplicationModelUpdateEvent(isComplete);
        }

        /// <summary>
        /// Load utterances.
        /// </summary>
        /// <returns></returns>
        public async Task LoadUtterancesAsync()
        {
            await _wordSource.LoadUtterancesAsync();
        }

        private bool AreAdjacentIndices(int lowerIndex, int upperIndex)
        {
            var adjustedLowerIndex = lowerIndex;
            while (adjustedLowerIndex < upperIndex && !Source.Filter.IsIndexVisible(adjustedLowerIndex))
            {
                adjustedLowerIndex++;
            }

            return adjustedLowerIndex == upperIndex;
        }

        internal void SaveUtterance(TileSequence sequence)
        {
            var utterance = new UtteranceData(sequence, _utteranceStartTime, _utteranceDuration, _utteranceActivationCount);
            var utteranceString = utterance.ToLine();
            _ = Environment.SaveTraceAsync(utteranceString);
            _ = Environment.SaveUtteranceAsync(utteranceString);
        }

        /// <summary>
        /// Save an utterance.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="isArtificial">Was data generated automatically.</param>
        public async Task SaveUtteranceAsync(TileSequence sequence, bool isArtificial = false)
        {
            var utterance = isArtificial ? new UtteranceData(sequence, true) : new UtteranceData(sequence, _utteranceStartTime, _utteranceDuration, _utteranceActivationCount);
            var utteranceString = utterance.ToLine();
            await Environment.SaveTraceAsync(utteranceString);
            await Environment.SaveUtteranceAsync(utteranceString);
        }

        internal void Trace(Command item)
        {
            var timestamp = Environment.GetTimestamp();

            var trace = XmlHelper.WriteXmlFragment(writer =>
            {
                var type = item.GetType();
                var name = type.Name;
                writer.WriteStartElement(name);

                writer.WriteAttributeString("TS", timestamp.ToString("o"));

                item.TraceContent(writer);
            });

            _ = Environment.SaveTraceAsync(trace);
            Debug.WriteLine(trace);
        }

        /// <summary>
        /// Debug function to show all tile types.
        /// </summary>
        public void ShowTestCard()
        {
            _wordSource.ShowTestCard();

            _suggestionInterstitials.Clear();

            var interstitialGapItem = new InterstitialGapItem(HeadItems[1], this, _wordSource, 1, 2);
            _suggestionInterstitials.Add(interstitialGapItem);
            _suggestionInterstitials.Add(interstitialGapItem);

            _suggestionInterstitials.Add(new InterstitialNonItem(this));

            var interstitialSpellingItem = new InterstitialSpellingItem(HeadItems[1], _wordSource.SpellingSource, 0);
            _suggestionInterstitials.Add(interstitialSpellingItem);
            _suggestionInterstitials.Add(interstitialSpellingItem);

            _suggestionInterstitials.Add(new InterstitialNonItem(this));

            var interstitialUnicodeItem = new InterstitialUnicodeItem(HeadItems[1], _wordSource.SpellingSource.UnicodeSource);
            _suggestionInterstitials.Add(interstitialUnicodeItem);
            _suggestionInterstitials.Add(interstitialUnicodeItem);

            _nextSuggestions.Clear();
            _nextSuggestions.Add(new ITile[] { new CommandItem(HeadItems[1], _wordSource, TileCommand.Typing) });
            _nextSuggestions.Add(new ITile[] { new ReplacementItem((HeadWordItem)HeadItems[1], _wordSource, "Replacement") });

            _nextSuggestions.Add(new ITile[] { new SuggestedWordItem(HeadItems[1], _wordSource, "Suggestion") });

            var suggestedWordItem1 = new SuggestedWordItem(HeadItems[1], _wordSource, "One");
            var suggestedWordItem2 = new SuggestedWordItem(suggestedWordItem1, _wordSource, "Two");
            var suggestedWordItem3 = new SuggestedWordItem(suggestedWordItem2, _wordSource, ".\0A");
            var tailStopItem = new TailStopItem(suggestedWordItem3, _wordSource);
            _nextSuggestions.Add(new ITile[] { suggestedWordItem1, suggestedWordItem2, suggestedWordItem3, tailStopItem });

            _nextSuggestions.Add(new ITile[] { new SuggestedSpellingItem(HeadItems[1], _wordSource.SpellingSource, "Hell", "o") });

            var suggestedSpellingItem = new SuggestedSpellingItem(HeadItems[1], _wordSource.SpellingSource, "Hel", "l");
            var suggestedSpellinSequenceItem = new SuggestedSpellingSequenceItem(suggestedSpellingItem, _wordSource.SpellingSource, "Hell", "o");
            var suggestedSpellingWordItem = new SuggestedSpellingWordItem(HeadItems[1], _wordSource, "Hello");
            _nextSuggestions.Add(new ITile[] { suggestedSpellingItem, suggestedSpellinSequenceItem, suggestedSpellingWordItem });

            _nextSuggestions.Add(new ITile[] { new SuggestedSpellingBackspaceItem(HeadItems[1], _wordSource.SpellingSource, "Hell") });

            _wordSource.SpellingSource.SetContext(new int[] { 0, 'H', 'e', 'l', 'l', 'o' });
            _nextSuggestions.Add(new ITile[] { new SuggestedUnicodeItem(HeadItems[1], _wordSource.SpellingSource, 33) });
        }

        private void RefreshSuggestionsView()
        {
            Source.SetSuggestionsView(_lowerBound, _upperLimit, false);
        }

        public async Task<bool> ShowSettingsAync()
        {
            var updated = await Environment.ShowSettingsAsync(Environment.Settings);
            if (updated)
            {
                RefreshSuggestionsView();
            }
            return updated;
        }
    }
}
