using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Outer spelling vocabulary source.
    /// </summary>
    public class OuterSpellingVocabularySource : VocabularySource
    {
        private readonly WordVocabularySource _wordVocabularySource;
        private readonly SpellingVocabularySource _spellingVocabularySource;

        private readonly UnicodeVocabularySource _unicodeVocabularySource;

        internal OuterSpellingVocabularySource(ApplicationModel model, WordVocabularySource wordVocabularySource)
            : base(model)
        {
            _wordVocabularySource = wordVocabularySource;
            _spellingVocabularySource = new SpellingVocabularySource(model, wordVocabularySource, this);

            _unicodeVocabularySource = new UnicodeVocabularySource(model, this);
        }

        internal string Prefix => _spellingVocabularySource.Prefix;

        private int HeaderSize => Math.Min(2, _spellingVocabularySource.Prefix.Length);

        internal override int Count => _spellingVocabularySource.Count + HeaderSize;

        internal void AddNewWord(string word)
        {
            _spellingVocabularySource.AddNewWord(word);
        }

        internal override IEnumerable<ICommand> CreateSuggestionList(int index)
        {
            if (index < HeaderSize)
            {
                if (index == HeaderSize - 1)
                {
                    yield return new SuggestedWordItem(_wordVocabularySource, _spellingVocabularySource.Prefix);
                }
                else
                {
                    yield return new SuggestedSpellingBackspaceItem(_spellingVocabularySource, _spellingVocabularySource.Prefix);
                }
            }
            else
            {
                Debug.Assert(index - HeaderSize < _spellingVocabularySource.Count);

                var enumerable = _spellingVocabularySource.CreateSuggestionList(index - HeaderSize);
                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
        }

        internal override ICommand CreatePriorInterstitial(int index)
        {
            ICommand interstitial;

            if (index < HeaderSize)
            {
                interstitial = null;
            }
            else
            {
                interstitial = new InterstitialUnicodeItem(_unicodeVocabularySource);
            }

            return interstitial;
        }

        internal override IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count)
        {
            var remainingCount = count;

            for (var i = HeaderSize - 1; minIndex <= i; i--)
            {
                yield return i;
                remainingCount--;
            }

            var remainder = _spellingVocabularySource.GetTopIndices(Math.Max(0, minIndex - HeaderSize), limIndex - HeaderSize, remainingCount);

            foreach (var index in remainder)
            {
                if (0 <= index)
                {
                    yield return index + HeaderSize;
                    remainingCount--;
                }
            }
        }

        internal void StartSpelling(int index)
        {
            _spellingVocabularySource.StartSpelling(index);
            SetSuggestionsView();
        }

        internal void AddSymbol(string symbol)
        {
            var ch = char.ConvertToUtf32(symbol, 0);
            _spellingVocabularySource.AddSpellingToken(ch);
        }
    }
}
