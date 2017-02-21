using Microsoft.HandsFree.Prediction.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    [TestClass]
    public class WordAndPunctuationHelperTest
    {
        const bool ShowWorkings = false;

        static string GetLocalHistoryText()
        {
            var filePath = SettingsDirectory.GetDefaultSettingsFilePath("spoken.txt");

            var records = XmlFragmentHelper.ReadLog<Spoken>(filePath);

            var text = string.Join(Environment.NewLine, from r in records select r.Text);

            return text;
        }

        [TestMethod]
        public void MakePairs()
        {
            var history = GetLocalHistoryText();
            var words = WordSource.ExtractHistoryWords(history);

            var outerDictionary = new SortedDictionary<string, IDictionary<string, int>>();

            using (var enumerator = words.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    var followingWord = enumerator.Current.ToLowerInvariant();

                    while (enumerator.MoveNext())
                    {
                        var precedingWord = followingWord;
                        followingWord = enumerator.Current.ToLowerInvariant();

                        IDictionary<string, int> innerDictionary;
                        if (!outerDictionary.TryGetValue(precedingWord, out innerDictionary))
                        {
                            innerDictionary = new SortedDictionary<string, int>();
                            outerDictionary.Add(precedingWord, innerDictionary);
                        }

                        int pairCount;
                        if (innerDictionary.TryGetValue(followingWord, out pairCount))
                        {
                            innerDictionary[followingWord] = pairCount + 1;
                        }
                        else
                        {
                            innerDictionary.Add(followingWord, 1);
                        }
                    }
                }
            }

            foreach (var outerPair in outerDictionary)
            {
                foreach (var innerPair in outerPair.Value)
                {
                    Debug.WriteLineIf(ShowWorkings, $"{outerPair.Key} {innerPair.Key} {innerPair.Value}");
                }
            }
        }

        [TestMethod]
        public void BreakIntoWordsAndPunctuation()
        {
            var history = GetLocalHistoryText();

            var index = 0;
            while (index < history.Length)
            {
                var punctuationLength = WordAndPunctuationHelper.PunctuationLength(history, index);
                var punctuation = history.Substring(index, punctuationLength);
                index += punctuationLength;

                var wordLength = WordAndPunctuationHelper.WordLength(history, index);
                var word = history.Substring(index, wordLength);
                index += wordLength;

                Debug.WriteLineIf(ShowWorkings, $"{punctuation} - {word}");
            }
        }

        static void CheckWordsAndPunctuation(string text, params string[] parts)
        {
            Assert.IsTrue(parts.Length % 2 == 0, "Must have pairings of word and punctuation");

            var textIndex = 0;
            var partsIndex = 0;
            while (textIndex < text.Length)
            {
                var wordLength = WordAndPunctuationHelper.WordLength(text, textIndex);
                var unexpectedPunctuationLength = WordAndPunctuationHelper.PunctuationLength(text, textIndex);
                Assert.IsTrue((wordLength == 0) || (unexpectedPunctuationLength == 0), "Must be a word or punctuation, but not both");

                var word = text.Substring(textIndex, wordLength);
                textIndex += wordLength;

                var reverseWordLength = WordAndPunctuationHelper.ReverseWordLength(text, textIndex);
                Assert.AreEqual(reverseWordLength, wordLength, "Forwards and backwards word length results should match");

                var unexpectedWordLength = WordAndPunctuationHelper.WordLength(text, textIndex);
                var punctuationLength = WordAndPunctuationHelper.PunctuationLength(text, textIndex);
                Assert.IsTrue(unexpectedWordLength == 0, "Cannot see a word after a preceeding word without punctuation");

                var punctuation = text.Substring(textIndex, punctuationLength);
                textIndex += punctuationLength;

                var reversePunctuationLength = WordAndPunctuationHelper.ReversePunctuationLength(text, textIndex);
                Assert.AreEqual(punctuationLength, reversePunctuationLength, "Forwards and backwards puctuation results should match");

                Assert.AreEqual(parts[partsIndex + 0], word, "Expected word");
                Assert.AreEqual(parts[partsIndex + 1], punctuation, "Expected punctuation");
                partsIndex += 2;
            }

            Assert.AreEqual(parts.Length, partsIndex, "Should consume all expected parts");
        }

        [TestMethod]
        public void ExerciseWordsAndPunctuationSeparation()
        {
            CheckWordsAndPunctuation("");
            CheckWordsAndPunctuation("A", "A", "");
            CheckWordsAndPunctuation("!", "", "!");
            CheckWordsAndPunctuation("Hazel O'Connor", "Hazel", " ", "O'Connor", "");
            CheckWordsAndPunctuation("'What!', exclaimed Roger.", "", "'", "What", "!', ", "exclaimed", " ", "Roger", ".");
            CheckWordsAndPunctuation("'What'", "", "'", "What", "'");
        }

        static void Add(IDictionary<string, IDictionary<string, int>> outerDictionary, string first, string second)
        {
            IDictionary<string, int> innerDictionary;
            if (!outerDictionary.TryGetValue(first, out innerDictionary))
            {
                innerDictionary = new SortedDictionary<string, int>();
                outerDictionary.Add(first, innerDictionary);
            }

            int pairCount;
            if (innerDictionary.TryGetValue(second, out pairCount))
            {
                innerDictionary[second] = pairCount + 1;
            }
            else
            {
                innerDictionary.Add(second, 1);
            }
        }

        [TestMethod]
        public void MakePairs3()
        {
            var history = GetLocalHistoryText();

            var outerDictionary = new SortedDictionary<string, IDictionary<string, int>>();

            var firstWord = string.Empty;

            var index = history.PunctuationLength(0);

            while (index < history.Length)
            {
                var secondWordLength = history.WordLength(index);
                var secondWord = history.Substring(index, secondWordLength).ToLowerInvariant();
                index += secondWordLength;

                var punctuationLength = history.PunctuationLength(index);
                var punctuation = history.Substring(index, punctuationLength);
                index += punctuationLength;

                Assert.AreNotEqual(0, secondWordLength);
                Add(outerDictionary, firstWord, secondWord);

                if (punctuation.IsSentenceEnding())
                {
                    firstWord = string.Empty;
                }
                else
                {
                    firstWord = secondWord;
                }
            }

            var outerHashSum = 0;
            var innerHashSum = 0;
            var countSum = 0;
            foreach (var outerPair in outerDictionary)
            {
                foreach (var innerPair in outerPair.Value)
                {
                    Debug.WriteLineIf(ShowWorkings, $"{outerPair.Key} {innerPair.Key} {innerPair.Value}");
                    outerHashSum += outerPair.GetHashCode();
                    innerHashSum += innerPair.GetHashCode();
                    countSum += innerPair.Value; ;
                }
            }
            Debug.WriteLineIf(ShowWorkings, $"Outer = {outerHashSum}, Inner = {innerHashSum}, Total = {countSum}");
        }
    }
}
