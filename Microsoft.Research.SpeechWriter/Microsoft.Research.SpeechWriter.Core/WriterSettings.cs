using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Research.SpeechWriter.Core
{
    public class WriterSettings : INotifyPropertyChanged
    {
        /// <summary>
        /// Speak each word as they are selected.
        /// </summary>
        public bool SpeakWordByWord
        {
            get => _speakWordByWord;
            set => SetProperty(ref _speakWordByWord, value);
        }
        private bool _speakWordByWord = true;

        /// <summary>
        /// Speak whole utterances as they are accepted.
        /// </summary>
        public bool SpeakWholeUtterances
        {
            get => _speakWholeUtterances;
            set => SetProperty(ref _speakWholeUtterances, value);
        }
        private bool _speakWholeUtterances = false;

        /// <summary>
        /// Find words that are most likely to follow core prediction words.
        /// </summary>
        public bool FindFollowOnPredictions
        {
            get => _findFollowOnPredictions;
            set => SetProperty(ref _findFollowOnPredictions, value);
        }
        private bool _findFollowOnPredictions = true;

        /// <summary>
        /// If the core predictions contain adjacent predictions, one of which is a prefix of the other, consider combining them.
        /// </summary>
        public bool CombineCorePredictions
        {
            get => _combineCorePredictions;
            set => SetProperty(ref _combineCorePredictions, value);
        }
        private bool _combineCorePredictions = true;

        /// <summary>
        /// Find valid prefixes to words in the core prediction.
        /// </summary>
        public bool FindCorePredictionPrefixes
        {
            get => _findCorePredictionPrefixes;
            set => SetProperty(ref _findCorePredictionPrefixes, value);
        }
        private bool _findCorePredictionPrefixes;

        /// <summary>
        /// Find valid suffixes to words in the core prediction.
        /// </summary>
        public bool FindCorePredictionSuffixes
        {
            get => _findCorePredictionSuffixes;
            set => SetProperty(ref _findCorePredictionSuffixes, value);
        }
        private bool _findCorePredictionSuffixes;

        /// <summary>
        /// Show interstitials allowing unknown words to be spelled.
        /// </summary>
        public bool ShowSpellingInterstitials
        {
            get => _showSpellingInterstitials;
            set => SetProperty(ref _showSpellingInterstitials, value);
        }
        private bool _showSpellingInterstitials = true;

        /// <summary>
        /// Show iterstitials allowing the entry of arbitrary unicode values.
        /// </summary>
        public bool ShowUnicodeInterstitials
        {
            get => _showUnicodeInterstitials;
            set => SetProperty(ref _showUnicodeInterstitials, value);
        }
        private bool _showUnicodeInterstitials = true;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _propertyChanged += value;
            remove => _propertyChanged -= value;
        }
        private PropertyChangedEventHandler _propertyChanged;


        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;

                _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
