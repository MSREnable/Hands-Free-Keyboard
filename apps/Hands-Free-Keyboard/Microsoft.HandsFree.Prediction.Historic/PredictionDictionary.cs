using Microsoft.HandsFree.Prediction.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.HandsFree.Prediction.Historic
{
    public class PredictionDictionary : IWordSuggester, IPhraseSuggester
    {
        const int WordUsesBeforeUsingInSuggestions = 2;

        class IntArray4Comparer : IEqualityComparer<int[]>
        {
            internal static readonly IntArray4Comparer Instance = new IntArray4Comparer();

            public bool Equals(int[] x, int[] y)
            {
                Debug.Assert(x.Length == 4);
                Debug.Assert(y.Length == 4);

                return x[0] == y[0] &&
                    x[1] == y[1] &&
                    x[2] == y[2] &&
                    x[3] == y[3];
            }

            public int GetHashCode(int[] obj)
            {
                return obj[0].GetHashCode() ^
                    obj[1].GetHashCode() ^
                    obj[2].GetHashCode() ^
                    obj[3].GetHashCode();
            }
        }

        List<DictionaryEntry> _dictionaryList = new List<DictionaryEntry>();

        int _wordCount;
        List<WordEntry> _wordList = new List<WordEntry>();
        Dictionary<string, int> _wordIndexDictionary = new Dictionary<string, int>();
        Dictionary<string, int> _temporaryIndexDictionary = new Dictionary<string, int>();

        Dictionary<int[], WeakReference<KeyScoreOrderedList>> _cache = new Dictionary<int[], WeakReference<KeyScoreOrderedList>>(IntArray4Comparer.Instance);

        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        PredictionDictionary()
        {
            // Empty constructor!
        }

        internal bool IsBefore(int[] lhs, int[] rhs)
        {
            bool isBefore;

            var position = 0;
            var minCount = Math.Min(lhs.Length, rhs.Length);

            while (position < minCount && lhs[position] == rhs[position])
            {
                position++;
            }

            if (position < minCount)
            {
                isBefore = string.Compare(_wordList[lhs[position]].Word, _wordList[rhs[position]].Word) < 0;
            }
            else if (position == lhs.Length)
            {
                isBefore = position != rhs.Length;
            }
            else
            {
                isBefore = false;
            }

            return isBefore;
        }

        int IndexWord(string word)
        {
            var normalisedWord = word.ToLowerInvariant();

            int index;
            if (!_wordIndexDictionary.TryGetValue(normalisedWord, out index) &&
                !_temporaryIndexDictionary.TryGetValue(normalisedWord, out index))
            {
                // Create a temporary index for the word until it gets used for real.
                index = 1 - _temporaryIndexDictionary.Count;
                _temporaryIndexDictionary.Add(normalisedWord, index);
            }

            return index;
        }

        int AddIndexWord(string word)
        {
            var normalisedWord = word.ToLowerInvariant();

            int index;
            if (_wordIndexDictionary.TryGetValue(normalisedWord, out index))
            {
                var entry = _wordList[index];
                if (entry.UseCount != byte.MaxValue)
                {
                    entry.UseCount++;
                }
            }
            else
            {
                index = _wordIndexDictionary.Count;

                var entry = new WordEntry { Word = normalisedWord, UseCount = 1 };
                _wordList.Add(entry);
                _wordIndexDictionary.Add(word, index);
            }

            return index;
        }

        public void AddPhrase(params string[] words)
        {
            _cache.Clear();

            for (var prefixLength = 0; prefixLength < words.Length; prefixLength++)
            {
                var prediction = AddIndexWord(words[prefixLength]);

                var prefix = new int[prefixLength];
                for (var wordIndex = 0; wordIndex < prefixLength; wordIndex++)
                {
                    prefix[wordIndex] = IndexWord(words[prefixLength - wordIndex - 1]);
                }

                var position = FindPhrasePosition(prefix);

                if (_dictionaryList.Count != 0 && (position != 0 && !IsBefore(_dictionaryList[position - 1].key, prefix) && !IsBefore(prefix, _dictionaryList[position - 1].key)))
                {
                    _wordCount++;
                    var predictionsList = _dictionaryList[position - 1].predictions;
                    predictionsList.Include(prediction, Math.Log(_wordCount));
                }
                else
                {
                    Debug.Assert(position == 0 || Comparer(_dictionaryList[position - 1].key, prefix) < 0);
                    Debug.Assert(position == _dictionaryList.Count || 0 <= Comparer(_dictionaryList[position].key, prefix));

                    var entry = new DictionaryEntry
                    {
                        predictions = new KeyScoreOrderedList(),
                        key = prefix
                    };
                    _dictionaryList.Insert(position, entry);

                    _wordCount++;
                    entry.predictions.Include(prediction, Math.Log(_wordCount));
                }
            }
        }

        int GetPrefixDepth(int[] reference, int index)
        {
            var candidate = _dictionaryList[index].key;

            var depth = 0;
            while (depth < reference.Length && depth < candidate.Length && reference[depth] == candidate[depth])
            {
                depth++;
            }

            return depth;
        }

        static int Comparer(int[] lhs, int[] rhs)
        {
            var position = 0;
            var limit = Math.Min(lhs.Length, rhs.Length);
            while (position < limit &&
                lhs[position] == rhs[position])
            {
                position++;
            }

            int comparison;

            if (position == lhs.Length)
            {
                comparison = rhs.Length != position ? -1 : 0;
            }
            else if (position == rhs.Length)
            {
                comparison = +1;
            }
            else
            {
                var lhsString = lhs[position];
                var rhsString = rhs[position];
                comparison = lhsString - rhsString;
            }

            return comparison;
        }

        int Comparer(int[] reference, int index)
        {
            var candidate = _dictionaryList[index].key;

            return Comparer(reference, candidate);
        }

        int FindPhrasePosition(int[] prefix)
        {
            var insertPosition = Plumber<int[]>.FindInsertPosition(prefix, Comparer, _dictionaryList.Count);

            return insertPosition;
        }

        [Conditional("DEBUG")]
        public void Dump()
        {
            for (var item = 0; item < _dictionaryList.Count; item++)
            {
                Debug.Write(string.Format("{0,10} :", item));
                foreach (var word in _dictionaryList[item].key)
                {
                    Debug.Write(" " + word);
                }
                Debug.Write(" =>");

                foreach (var pair in _dictionaryList[item].predictions.KeyScorePairs)
                {
                    Debug.Write(" " + pair.key);
                }
                Debug.WriteLine("");
            }
        }

        static bool PrefixMatch(int length, string[] lhs, string[] rhs)
        {
            var count = 0;

            if (length <= lhs.Length && length <= rhs.Length)
            {
                while (count < length && lhs[count] == rhs[count])
                {
                    count++;
                }
            }

            return count == length;
        }

        [Conditional("DEBUG")]
        void DumpPrefixes(int lowerPosition, int upperPosition)
        {
            if (lowerPosition < upperPosition)
            {
                if (lowerPosition + 1 == upperPosition)
                {
                    var key = _dictionaryList[lowerPosition].key;

                    Debug.Write("  :");
                    foreach (var word in key)
                    {
                        Debug.Write(" " + word);
                    }
                    Debug.Write(" =>");

                    foreach (var pair in _dictionaryList[lowerPosition].predictions.KeyScorePairs)
                    {
                        Debug.Write(" " + pair.key);
                    }

                    Debug.WriteLine("");
                }
                else
                {
                    var combined = new KeyScoreOrderedList();

                    for (var position = lowerPosition; position < upperPosition; position++)
                    {
                        foreach (var pair in _dictionaryList[position].predictions.KeyScorePairs)
                        {
                            combined.Include(pair.key, pair.score);
                        }
                    }

                    Debug.Write(" : * =>");

                    foreach (var pair in combined.KeyScorePairs)
                    {
                        Debug.Write(" " + pair.key);
                    }

                    Debug.WriteLine("");
                }
            }
        }

        KeyScoreOrderedList GetCombinedSliceSuggestions(int startLhs, int limitLhs, int startRhs, int limitRhs)
        {
            Debug.Assert(startLhs < limitLhs);
            Debug.Assert(limitLhs <= startRhs);
            Debug.Assert(startRhs <= limitRhs);

            var key = new[] { startLhs, limitLhs, startRhs, limitRhs };

            WeakReference<KeyScoreOrderedList> reference;
            _cache.TryGetValue(key, out reference);

            KeyScoreOrderedList list;

            if (reference == null || !reference.TryGetTarget(out list))
            {
                if (reference != null)
                {
                    Debug.WriteLine("Cache item garbage collected");
                }

                list = new KeyScoreOrderedList();

                for (var indexLhs = startLhs; indexLhs < limitLhs; indexLhs++)
                {
                    list.Include(_dictionaryList[indexLhs].predictions);
                }

                for (var indexRhs = startRhs; indexRhs < limitRhs; indexRhs++)
                {
                    list.Include(_dictionaryList[indexRhs].predictions);
                }

                _cache[key] = new WeakReference<KeyScoreOrderedList>(list);
            }
            else
            {
                Debug.WriteLine($"Cache hit! [{startLhs}..{limitLhs})={limitLhs - startLhs} & [{startRhs}..{limitRhs})={limitRhs - startRhs} :=> {limitLhs - startLhs + limitRhs - startRhs}");
            }

            return list;
        }

        IEnumerable<int> GetSliceSuggestions(int startLhs, int limitLhs, int startRhs, int limitRhs)
        {
            Debug.Assert(startLhs <= limitLhs);
            Debug.Assert(limitLhs <= startRhs);
            Debug.Assert(startRhs <= limitRhs);
            Debug.Assert(startRhs - limitLhs < limitRhs - startLhs);

            KeyScoreOrderedList orderedList;

            if ((limitLhs - startLhs) + (limitRhs - startRhs) == 1)
            {
                int index;

                if (startLhs == limitLhs)
                {
                    Debug.Assert(startRhs + 1 == limitRhs);

                    index = startRhs;
                }
                else
                {
                    Debug.Assert(startLhs + 1 == limitLhs & startRhs == limitRhs);

                    index = startLhs;
                }

                orderedList = _dictionaryList[index].predictions;
            }
            else if (startLhs == limitLhs)
            {
                orderedList = GetCombinedSliceSuggestions(startRhs, limitRhs, limitRhs, limitRhs);
            }
            else
            {
                orderedList = GetCombinedSliceSuggestions(startLhs, limitLhs, startRhs, limitRhs);
            }

            foreach (var pair in orderedList.KeyScorePairs)
            {
                yield return pair.key;
            }
        }

        public IEnumerable<int> MakePredictions(int count, string startsWith, params int[] prefix)
        {
            if (_dictionaryList.Count != 0)
            {
                var emitted = new HashSet<int>();
                var emittedCount = 0;

                var plumber = Plumber<int[]>.Create(prefix, GetPrefixDepth, Comparer, _dictionaryList.Count);
                var sliceEnumerable = plumber.GetDepthSlices();
                using (var sliceEnumerator = sliceEnumerable.GetEnumerator())
                {
                    if (sliceEnumerator.MoveNext())
                    {
                        var outerSlice = sliceEnumerator.Current;

                        var suggestions = GetSliceSuggestions(outerSlice.Start, outerSlice.Limit, outerSlice.Limit, outerSlice.Limit);

                        using (var enumerator = suggestions.GetEnumerator())
                        {
                            while (emittedCount < count && enumerator.MoveNext())
                            {
                                var word = enumerator.Current;
                                var wordEntry = _wordList[word];
                                var wordString = wordEntry.Word;

                                if (WordUsesBeforeUsingInSuggestions <= wordEntry.UseCount && wordString.StartsWith(startsWith))
                                {
                                    Debug.Assert(!emitted.Contains(word));
                                    emitted.Add(word);

                                    yield return word;
                                    emittedCount++;
                                }
                            }
                        }

                        while (emittedCount < count && sliceEnumerator.MoveNext())
                        {
                            var innerSlice = outerSlice;
                            outerSlice = sliceEnumerator.Current;

                            suggestions = GetSliceSuggestions(outerSlice.Start, innerSlice.Start, innerSlice.Limit, outerSlice.Limit);

                            using (var enumerator = suggestions.GetEnumerator())
                            {
                                while (emittedCount < count && enumerator.MoveNext())
                                {
                                    var word = enumerator.Current;
                                    var wordEntry = _wordList[word];
                                    var wordString = wordEntry.Word;

                                    if (WordUsesBeforeUsingInSuggestions <= wordEntry.UseCount && wordString.StartsWith(startsWith) && !emitted.Contains(word))
                                    {
                                        emitted.Add(word);

                                        yield return enumerator.Current;
                                        emittedCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        int PredictNextPhraseWord(List<int> prefix)
        {
            int word;

            if (_dictionaryList.Count != 0)
            {
                var plumber = Plumber<int[]>.Create(prefix.ToArray(), GetPrefixDepth, Comparer, _dictionaryList.Count);
                var sliceEnumerable = plumber.GetDepthSlices();
                using (var sliceEnumerator = sliceEnumerable.GetEnumerator())
                {
                    var sliceMoveNext = sliceEnumerator.MoveNext();
                    Debug.Assert(sliceMoveNext, "There must be a first slice");

                    var slice = sliceEnumerator.Current;

                    if (slice.Start != 0 || slice.Limit != _dictionaryList.Count)
                    {
                        var suggestions = GetSliceSuggestions(slice.Start, slice.Limit, slice.Limit, slice.Limit);

                        using (var enumerator = suggestions.GetEnumerator())
                        {
                            var suggestionMoveNext = enumerator.MoveNext();
                            Debug.Assert(suggestionMoveNext, "There must be a first suggestion");

                            word = enumerator.Current;

                            prefix.Insert(0, word);
                        }
                    }
                    else
                    {
                        word = -1;
                    }
                }
            }
            else
            {
                word = -1;
            }

            return word;
        }

        public IEnumerable<int> MakePhrasePrediction(int[] previousWords)
        {
            var prefix = new List<int>(previousWords);

            var count = 0;
            for (var word = PredictNextPhraseWord(prefix); word != -1 && count < 6; count++, word = PredictNextPhraseWord(prefix))
            {
                yield return word;
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((Int32)_wordCount);

            writer.Write((Int32)_wordList.Count);
            foreach (var entry in _wordList)
            {
                entry.Write(writer);
            }

            writer.Write((Int32)_dictionaryList.Count);

            foreach (var entry in _dictionaryList)
            {
                entry.Write(writer);
            }
        }

        public static PredictionDictionary Load(BinaryReader reader)
        {
            var wordCount = reader.ReadInt32();

            var wordListLength = reader.ReadInt32();
            var wordList = new List<WordEntry>(wordListLength);
            var wordIndexDictionary = new Dictionary<string, int>(wordListLength);
            for (var wordIndex = 0; wordIndex < wordListLength; wordIndex++)
            {
                var entry = WordEntry.Read(reader);
                wordList.Add(entry);
                wordIndexDictionary.Add(entry.Word, wordIndex);
            }

            var dictionaryListCount = reader.ReadInt32();

            var dictionaryList = new List<DictionaryEntry>(dictionaryListCount);
            for (var index = 0; index < dictionaryListCount; index++)
            {
                var entry = DictionaryEntry.Read(reader);
                dictionaryList.Add(entry);
            }

            var dictionary = new PredictionDictionary { _wordCount = wordCount, _dictionaryList = dictionaryList, _wordList = wordList, _wordIndexDictionary = wordIndexDictionary };

            return dictionary;
        }

        public void Save(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                Save(writer);
            }
        }

        public static PredictionDictionary Load(Stream stream)
        {
            PredictionDictionary dictionary;

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                dictionary = Load(reader);
            }

            return dictionary;
        }

        public void AddRawPhrases(string history)
        {
            try
            {
                _semaphore.Wait();

                var position = history.PunctuationLength(0);
                while (position < history.Length)
                {
                    var phraseWords = new List<string>();

                    for (var sentenceEnd = false; !sentenceEnd && position < history.Length;)
                    {
                        var wordLength = history.WordLength(position);
                        var word = history.Substring(position, wordLength);
                        position += wordLength;

                        var punctuationLength = history.PunctuationLength(position);
                        var punctuation = history.Substring(position, punctuationLength);
                        position += punctuationLength;

                        var lowercaseWord = word.ToLowerInvariant();
                        phraseWords.Add(lowercaseWord);

                        sentenceEnd = punctuation.IsSentenceEnding();
                    }

                    AddPhrase(phraseWords.ToArray());
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static PredictionDictionary Create(IPredictionEnvironment environment)
        {
            PredictionDictionary dictionary;

            try
            {
                using (var stream = environment.OpenDynamicDictionaryCache())
                {
                    if (stream != null)
                    {
                        dictionary = Load(stream);
                    }
                    else
                    {
                        dictionary = null;
                    }
                }
            }
            catch
            {
                dictionary = null;
            }

            if (dictionary == null)
            {
                dictionary = new PredictionDictionary();

                var history = environment.GetHistoryText();
                dictionary.AddRawPhrases(history);

                using (var stream = environment.CreateDynamicDictionaryCache())
                {
                    dictionary.Save(stream);
                }
            }

            return dictionary;
        }

        public IEnumerable<string> GetSuggestions(string[] previousWords, string currentWordPrefix)
        {
            IEnumerable<int> predictions;

            try
            {
                _semaphore.Wait();

                var lowercasePreviousWords = new int[previousWords.Length];
                for (var i = 0; i < previousWords.Length; i++)
                {
                    lowercasePreviousWords[i] = IndexWord(previousWords[i]);
                }

                predictions = MakePredictions(7, currentWordPrefix, lowercasePreviousWords);
            }
            finally
            {
                _semaphore.Release();
            }

            foreach (var prediction in predictions)
            {
                yield return _wordList[prediction].Word;
            }
        }

        public IEnumerable<string> GetPhraseSuggestion(string[] previousWords)
        {
            IEnumerable<int> prediction;

            try
            {
                _semaphore.Wait();

                var lowercasePreviousWords = new int[previousWords.Length];

                for (var i = 0; i < previousWords.Length; i++)
                {
                    lowercasePreviousWords[i] = IndexWord(previousWords[i]);
                }

                prediction = MakePhrasePrediction(lowercasePreviousWords);
            }
            finally
            {
                _semaphore.Release();
            }

            foreach (var index in prediction)
            {
                yield return _wordList[index].Word;
            }
        }
    }
}
