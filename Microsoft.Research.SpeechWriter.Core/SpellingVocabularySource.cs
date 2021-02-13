using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// The spelling source.
    /// </summary>
    public class SpellingVocabularySource : PredictiveVocabularySource
    {
        private const int SeedSequenceWeight = 1;

        private readonly List<int> _vocabularyList = new List<int>();

        private readonly Dictionary<int, int> _tokenToIndex = new Dictionary<int, int>();

        private readonly SortedSet<string> _characterSet;

        private readonly WordVocabularySource _wordVocabularySource;

        private readonly OuterSpellingVocabularySource _outerSpellingVocabularySource;

        /// <summary>
        /// The spelling context.
        /// </summary>
        private IList<int> Context { get; } = new List<int>();

        internal SpellingVocabularySource(ApplicationModel model, WordVocabularySource wordVocabularySource, OuterSpellingVocabularySource outerSpellingVocabularySource)
            : base(model: model, predictorWidth: 4)
        {
            _wordVocabularySource = wordVocabularySource;
            _outerSpellingVocabularySource = outerSpellingVocabularySource;

            _characterSet = new SortedSet<string>(Environment);

            foreach (var word in _wordVocabularySource.Words)
            {
                var sequence = new List<int>(word.Length + 2);
                sequence.Add(0);
                foreach (var ch in word.ToCharArray())
                {
                    sequence.Add(ch);
                }
                sequence.Add(0);

                PersistantPredictor.AddSequence(sequence, SeedSequenceWeight);
            }

            PopulateVocabularyList();
        }

        internal override int[] GetContext() => Context.ToArray();

        /// <summary>
        /// The number of items within the source.
        /// </summary>
        internal override int Count => _vocabularyList.Count;

        internal void AddNewWord(string word)
        {
            var needToRepopuplate = false;

            var sequence = new List<int>(word.Length + 2);
            sequence.Add(0);
            for (var index = 0; index < word.Length;)
            {
                int utf32;

                var ch = word[index];
                if (char.GetUnicodeCategory(ch) != UnicodeCategory.Surrogate)
                {
                    utf32 = ch;
                    index++;
                }
                else
                {
                    utf32 = char.ConvertToUtf32(word, index);
                    index += 2;
                }

                sequence.Add(utf32);
                if (_characterSet.Add(char.ConvertFromUtf32(utf32)))
                {
                    needToRepopuplate = true;
                }
            }
            sequence.Add(0);

            PersistantPredictor.AddSequence(sequence, SeedSequenceWeight);

            if (needToRepopuplate)
            {
                PopulateVocabularyList();
            }
        }

        private void PopulateVocabularyList()
        {
            var tokens = GetOrderedVocabularyListTokens();

            _vocabularyList.Clear();
            _tokenToIndex.Clear();

            var index = 0;
            foreach (var token in tokens)
            {
                Debug.Assert(index == _vocabularyList.Count);

                _vocabularyList.Add(token);
                _tokenToIndex.Add(token, index);

                index++;
            }
        }

        /// <summary>
        /// Get the non-zero token for at a given index within the source.
        /// </summary>
        /// <param name="index">Index of item, such that 0 &gt;= index &gt; Count.</param>
        /// <returns>The token at the give index</returns>
        internal override int GetIndexToken(int index)
        {
            return index < 0 ? 0 : _vocabularyList[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetOrderedVocabularyListTokens()
        {
            foreach (var token in PersistantPredictor.Tokens)
            {
                _characterSet.Add(char.ConvertFromUtf32(token));
            }
            _characterSet.Remove(((char)0).ToString());

            foreach (var token in _characterSet)
            {
                yield return char.ConvertToUtf32(token, 0);
            }
        }

        internal string Prefix
        {
            get
            {
                var prefix = string.Empty;
                for (var i = 1; i < Context.Count; i++)
                {
                    prefix += (char)Context[i];
                }
                return prefix;
            }
        }

        private ICommand GetTokenItem(int token)
        {
            var prefix = Prefix;

            var item = new SuggestedSpellingItem(this, prefix, token);

            return item;
        }

        internal override int GetTokenIndex(int token)
        {
            int index;

            if (token == 0)
            {
                index = -1;
            }
            else
            {
                index = _tokenToIndex[token];
            }

            return index;
        }

        internal override ICommand GetIndexItem(int index)
        {
            var token = GetIndexToken(index);
            var item = GetTokenItem(token);
            return item;
        }

        internal override ICommand GetSequenceItem(IEnumerable<int> tokenSequence)
        {
            ICommand item;

            var sequence = tokenSequence.ToArray();
            var tail = sequence[sequence.Length - 1];

            if (tail == 0)
            {
                var word = Prefix;
                for (var i = 0; i < sequence.Length - 1; i++)
                {
                    word += (char)sequence[i];
                }
                item = new SuggestedWordItem(_wordVocabularySource, word);
            }
            else
            {
                item = new SuggestedSpellingSequenceItem(this, sequence);
            }

            return item;
        }

        internal override IEnumerable<int> GetTokens()
        {
            foreach (var token in _vocabularyList)
            {
                yield return token;
            }
        }

        internal void StartSpelling(int index)
        {
            Context.Clear();
            Context.Add(0);

            if (index != 0 && index != _wordVocabularySource.Count)
            {
                var beforeWord = _wordVocabularySource.GetIndexWord(index - 1);
                var afterWord = _wordVocabularySource.GetIndexWord(index);
                var maxLength = Math.Min(beforeWord.Length, afterWord.Length);
                for (var i = 0; i < maxLength && beforeWord[i] == afterWord[i]; i++)
                {
                    Context.Add(beforeWord[i]);
                }
            }
        }

        internal void AddSpellingToken(int token)
        {
            Context.Add(token);

            if(_characterSet.Add(char.ConvertFromUtf32(token)))
            {
                PopulateVocabularyList();
            }

            _outerSpellingVocabularySource.SetSuggestionsView();
        }

        internal void AddSpellingTokens(int[] tokens)
        {
            foreach (var token in tokens)
            {
                Context.Add(token);
            }
            ResetSuggestionsView();
        }

        internal void SpellingBackspace()
        {
            var contextCount = Context.Count;

            if (contextCount == 2)
            {
                _wordVocabularySource.SetSuggestionsView();
            }
            else
            {
                Context.RemoveAt(contextCount - 1);
                ResetSuggestionsView();
            }
        }
    }
}