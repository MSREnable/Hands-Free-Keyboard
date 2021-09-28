using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// The spelling source.
    /// </summary>
    public class SpellingVocabularySource : PredictiveVocabularySource<ISuggestionItem>, ITokenTileFilter
    {
        private const int HeaderSize = 3;

        private readonly List<int> _vocabularyList = new List<int>();

        private readonly Dictionary<int, int> _tokenToIndex = new Dictionary<int, int>();

        private readonly SortedSet<string> _characterSet;

        private readonly WordVocabularySource _wordVocabularySource;

        private readonly UnicodeVocabularySource _unicodeVocabularySource;

        internal SpellingVocabularySource(ApplicationModel model, WordVocabularySource wordVocabularySource)
            : base(model: model, predictorWidth: 4)
        {
            _wordVocabularySource = wordVocabularySource;

            _characterSet = new SortedSet<string>(Environment);

            foreach (var word in _wordVocabularySource.Words)
            {
                var actualLength = word.Length;
                var effectiveLength = 0;
                while (effectiveLength < actualLength && word[effectiveLength] != 0)
                {
                    effectiveLength++;
                }
                if (effectiveLength != 0)
                {
                    var sequence = new int[effectiveLength + 2];

                    var index = 1;
                    for (var i = 0; i < effectiveLength; i++)
                    {
                        sequence[index] = word[i];
                        index++;
                    }
                    Debug.Assert(sequence[0] == 0);
                    Debug.Assert(sequence[effectiveLength + 2 - 1] == 0);

                    PersistantPredictor.AddSequence(sequence, WordVocabularySource.SeedSequenceWeight);
                }
            }

            foreach (var symbol in model.Environment.GetAdditionalSymbols())
            {
                var sequence = new int[] { symbol };
                PersistantPredictor.AddSequence(sequence, WordVocabularySource.SeedSequenceWeight);
            }

            _unicodeVocabularySource = new UnicodeVocabularySource(model, this);

            PopulateVocabularyList();
        }

        internal UnicodeVocabularySource UnicodeSource => _unicodeVocabularySource;

        /// <summary>
        /// The number of items within the source.
        /// </summary>
        internal override int Count => _vocabularyList.Count;

        internal override ITokenTileFilter TokenFilter => this;

        internal void AddNewWord(string word)
        {
            var needToRepopuplate = false;

            var sequence = new List<int>(word.Length + 2)
            {
                0
            };
            for (var index = 0; index < word.Length;)
            {
                int utf32 = char.ConvertToUtf32(word, index);
                if (char.GetUnicodeCategory(word, index) != UnicodeCategory.Surrogate)
                {
                    index++;
                }
                else
                {
                    index += 2;
                }

                sequence.Add(utf32);
                if (_characterSet.Add(char.ConvertFromUtf32(utf32)))
                {
                    needToRepopuplate = true;
                }
            }
            sequence.Add(0);

            PersistantPredictor.AddSequence(sequence, WordVocabularySource.SeedSequenceWeight);

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

            for (var i = 0; i < 3; i++)
            {
                var token = -i;

                Debug.Assert(index == _vocabularyList.Count);

                _vocabularyList.Add(token);
                _tokenToIndex.Add(token, index);

                index++;
            }

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
            return _vocabularyList[index];
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
                for (var i = 1; i < Context.Length; i++)
                {
                    prefix += (char)Context[i];
                }
                return prefix;
            }
        }

        bool ITileFilter.IsIndexVisible(int index)
        {
            var token = _vocabularyList[index];
            var visible = ((ITokenTileFilter)this).IsTokenVisible(token);
            return visible;
        }

        bool ITokenTileFilter.IsTokenVisible(int token)
        {
            bool visible;

            switch (token)
            {
                case 0:
                    visible = false;
                    break;

                case -1:
                    visible = 1 < Context.Length;
                    break;

                case -2:
                    visible = 1 < Context.Length;
                    break;

                default:
                    visible = true;
                    break;
            }

            return visible;
        }

        internal override int GetTokenIndex(int token)
        {
            var index = _tokenToIndex[token];

            return index;
        }

        internal override ISuggestionItem GetIndexItem(int index)
        {
            Debug.Assert(((ITileFilter)this).IsIndexVisible(index));

            var token = GetIndexToken(index);

            ISuggestionItem item;

            switch (token)
            {
                case -1:
                    item = new SuggestedSpellingBackspaceItem(Model.LastTile, this, Prefix);
                    break;
                case -2:
                    item = _wordVocabularySource.CreateSuggestedSpellingWordItem(Prefix);
                    break;
                default:
                    Debug.Assert(token != 0);
                    item = new SuggestedSpellingItem(Model.LastTile, this, Prefix, char.ConvertFromUtf32(token));
                    break;
            }

            return item;
        }

        internal override ITile GetIndexItemForTrace(int index)
        {
            var token = GetIndexToken(index);

            ISuggestionItem item;

            switch (token)
            {
                case -1:
                    item = new SuggestedSpellingBackspaceItem(Model.LastTile, this, Prefix);
                    break;
                case -2:
                    item = string.IsNullOrWhiteSpace(Prefix) ? null : _wordVocabularySource.CreateSuggestedSpellingWordItem(Prefix);
                    break;
                default:
                    item = token == 0 ? null : new SuggestedSpellingItem(Model.LastTile, this, Prefix, char.ConvertFromUtf32(token));
                    break;
            }

            return item;
        }

        internal ISuggestionItem GetNextItem(ITile predecessor, string prefix, int token)
        {
            ISuggestionItem item;

            if (token == 0)
            {
                item = _wordVocabularySource.CreateSuggestedWordItem(prefix);
            }
            else
            {
                item = new SuggestedSpellingSequenceItem(predecessor, this, prefix, char.ConvertFromUtf32(token));
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

        internal override IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count)
        {
            var effectiveMinIndex = minIndex;
            var effectiveCount = count;

            while (effectiveMinIndex < HeaderSize && effectiveMinIndex < limIndex && 0 < effectiveCount)
            {
                if (((ITileFilter)this).IsIndexVisible(effectiveMinIndex))
                {
                    yield return effectiveMinIndex;

                    effectiveCount--;
                }
                effectiveMinIndex++;
            }
            if (effectiveMinIndex < limIndex && 0 < effectiveCount)
            {
                var enumeration = base.GetTopIndices(effectiveMinIndex, limIndex, effectiveCount);
                foreach (var index in enumeration)
                {
                    yield return index;
                }
            }
        }

        internal override ITile CreatePriorInterstitial(int index)
        {
            ITile interstitial;

            if (index < HeaderSize)
            {
                interstitial = null;
            }
            else
            {
                interstitial = new InterstitialUnicodeItem(Model.LastTile, _unicodeVocabularySource);
            }

            return interstitial;
        }

        private static string RemoveAnySuffix(string decorated)
        {
            string value;

            var nullPosition = decorated.IndexOf('\0');
            if (nullPosition == -1)
            {
                value = decorated;
            }
            else
            {
                value = decorated.Substring(0, nullPosition);
            }

            return value;
        }

        internal void StartSpelling(int index)
        {
            var context = new List<int> { 0 };

            if (_wordVocabularySource.Count != index)
            {
                var afterWordSuffixed = _wordVocabularySource.GetIndexWord(index);
                var afterWord = RemoveAnySuffix(afterWordSuffixed);

                if (afterWord != string.Empty)
                {
                    string beforeWord;
                    var beforeIndex = index - 1;
                    do
                    {
                        if (0 <= beforeIndex)
                        {
                            var beforeWordSuffixed = _wordVocabularySource.GetIndexWord(beforeIndex);
                            beforeWord = RemoveAnySuffix(beforeWordSuffixed);
                            beforeIndex--;
                        }
                        else
                        {
                            beforeWord = string.Empty;
                        }
                    }
                    while (beforeWord == afterWord);

                    var maxLength = Math.Min(beforeWord.Length, afterWord.Length);
                    for (var i = 0; i < maxLength && beforeWord[i] == afterWord[i]; i++)
                    {
                        context.Add(beforeWord[i]);
                    }
                }
            }

            SetContext(context);
            SetSuggestionsView();
        }

        internal void AddSpellingToken(int token)
        {
            SetContext(new List<int>(Context) { token });

            if (_characterSet.Add(char.ConvertFromUtf32(token)))
            {
                PopulateVocabularyList();
            }
        }

        internal void SetSpellingPrefix(string prefix)
        {
            var context = new List<int> { 0 };

            var length = prefix.Length;
            var newChar = false;
            for (var index = 0; index < length;)
            {
                var ch = char.ConvertToUtf32(prefix, index);
                context.Add(ch);

                if (_characterSet.Add(char.ConvertFromUtf32(ch)))
                {
                    newChar = true;
                }

                index += char.IsSurrogate(prefix, index) ? 2 : 1;
            }

            SetContext(context);

            if (newChar)
            {
                PopulateVocabularyList();
            }

            SetSuggestionsView();
        }

        internal void SpellingBackspace()
        {
            var contextCount = Context.Length;

            if (contextCount == 1)
            {
                _wordVocabularySource.SetSuggestionsView();
            }
            else
            {
                var context = Context;
                Array.Resize(ref context, contextCount - 1);
                SetContext(context);

                SetSuggestionsView();
            }
        }

        internal void AddSymbol(string symbol)
        {
            var ch = char.ConvertToUtf32(symbol, 0);
            AddSpellingToken(ch);
            SetSuggestionsView();
        }
    }
}