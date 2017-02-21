using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.HandsFree.Prediction.Historic.Test
{
    [TestClass]
    public class PredictionDictionaryTest
    {
        static void CheckOrder(bool expected, params string[] nullSeparatedArrays)
        {
            var position = 0;

            var lhsList = new List<string>();
            while (nullSeparatedArrays[position] != null)
            {
                lhsList.Add(nullSeparatedArrays[position]);
                position++;
            }

            Assert.IsNull(nullSeparatedArrays[position]);
            position++;

            var rhsList = new List<string>();
            while (position < nullSeparatedArrays.Length)
            {
                rhsList.Add(nullSeparatedArrays[position]);
                position++;
            }

            //var actual = PredictionDictionary.IsBefore(lhsList.ToArray(), rhsList.ToArray());

            //Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TwoOrderedWords()
        {
            CheckOrder(true, "A", null, "B");
        }

        [TestMethod]
        public void TwoUnorderedWords()
        {
            CheckOrder(false, "B", null, "A");
        }

        [TestMethod]
        public void EmptyWords()
        {
            CheckOrder(false, new string[] { null });
        }

        [TestMethod]
        public void SameWords()
        {
            CheckOrder(false, "A", null, "A");
        }

        [TestMethod]
        public void BeforeBecauseShorter()
        {
            CheckOrder(true, "A", null, "A", "A");
        }

        [TestMethod]
        public void AfterBecauseLonger()
        {
            CheckOrder(false, "A", "A", null, "A");
        }

        [TestMethod]
        public void SimpleQuickBrownFox()
        {
            var environment = new TestPredictionEnvironment();
            var dictionary = PredictionDictionary.Create(environment);
            dictionary.AddPhrase("the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog");
            dictionary.Dump();
        }

        [TestMethod]
        public void DoubleQuickBrownFox()
        {
            var environment = new TestPredictionEnvironment();
            var dictionary = PredictionDictionary.Create(environment);
            for (var i = 0; i < 2; i++)
            {
                dictionary.AddPhrase("the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog");
            }
            dictionary.Dump();
        }

        [TestMethod]
        public void AllTestPhrases()
        {
            var environment = new TestPredictionEnvironment();
            var dictionary = PredictionDictionary.Create(environment);

            foreach (var sentence in TestSentences.Instance)
            {
                var position = sentence.PunctuationLength(0);

                var words = new List<string>();
                while (position < sentence.Length)
                {
                    var wordLength = sentence.WordLength(position);
                    var word = sentence.Substring(position, wordLength).ToLowerInvariant();
                    position += wordLength;
                    position += sentence.PunctuationLength(position);

                    words.Add(word);
                }

                dictionary.AddPhrase(words.ToArray());
            }

            dictionary.Dump();
        }

        [TestMethod]
        public void OrderNeutrality()
        {
            var environment = new TestPredictionEnvironment();

            var dictionaryAB = PredictionDictionary.Create(environment);
            dictionaryAB.AddPhrase("A");
            dictionaryAB.AddPhrase("B");

            var dictionaryBA = PredictionDictionary.Create(environment);
            dictionaryBA.AddPhrase("B");
            dictionaryBA.AddPhrase("A");
        }

        static void ExercisePrediction(PredictionDictionary dictionary, params string[] prefix)
        {
            for (var i = prefix.Length - 1; 0 <= i; i--)
            {
                Debug.Write(prefix[i] + " ");
            }
            Debug.Write(" =>");

            //foreach (var word in dictionary.MakePredictions(7, "", prefix))
            //{
            //    Debug.Write(" " + word);
            //}
            Debug.WriteLine("");
        }

        static PredictionDictionary CreateDictionary(IEnumerable<string> sentences)
        {
            var environment = new TestPredictionEnvironment();
            var dictionary = PredictionDictionary.Create(environment);

            foreach (var sentence in sentences)
            {
                var position = sentence.PunctuationLength(0);

                var words = new List<string>();
                while (position < sentence.Length)
                {
                    var wordLength = sentence.WordLength(position);
                    var word = sentence.Substring(position, wordLength).ToLowerInvariant();
                    position += wordLength;
                    position += sentence.PunctuationLength(position);

                    words.Add(word);
                }

                dictionary.AddPhrase(words.ToArray());
            }

            return dictionary;
        }

        static PredictionDictionary CreateDictionary(params string[] sentences)
        {
            var dictionary = CreateDictionary((IEnumerable<string>)sentences);
            return dictionary;
        }

        static PredictionDictionary CreateDictionary()
        {
            var dictionary = CreateDictionary(TestSentences.Instance);
            return dictionary;
        }

        [TestMethod]
        public void DictionarySearcher()
        {
            var dictionary = CreateDictionary();

            ExercisePrediction(dictionary, "brown", "quick", "the");
            ExercisePrediction(dictionary);
            ExercisePrediction(dictionary, "zzz");
            ExercisePrediction(dictionary, "kkk");
            ExercisePrediction(dictionary, "brown");
            ExercisePrediction(dictionary, "the");
        }

        static void ExercisePhrase(PredictionDictionary dictionary, string[] prefix, params string[] expecteds)
        {
            //var prefixReversed = prefix;
            //Array.Reverse(prefixReversed);

            //var enumerable = dictionary.MakePhrasePrediction(prefixReversed);

            //using (var enumerator = enumerable.GetEnumerator())
            //{
            //    var count = 0;

            //    for (; count < 6 && enumerator.MoveNext(); count++)
            //    {
            //        Assert.AreEqual(expecteds[count], enumerator.Current);
            //    }

            //    if (count != 6)
            //    {
            //        Assert.AreEqual(expecteds.Length, count);
            //    }
            //}
        }

        static void ExercisePhrase(PredictionDictionary dictionary, string prefixString, string expectedsString)
        {
            var prefix = prefixString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var expecteds = expectedsString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            ExercisePhrase(dictionary, prefix, expecteds);
        }

        [TestMethod]
        public void PhraseCreator()
        {
            var dictionary = CreateDictionary();

            ExercisePhrase(dictionary, "", "the fire raged for an entire");
            ExercisePhrase(dictionary, "the", "fire raged for an entire month");
            ExercisePhrase(dictionary, "questioning the", "wisdom of the courts");
            ExercisePhrase(dictionary, "microsoft", "");
        }

        [TestMethod]
        public void LazyPhraseCreator1()
        {
            var dictionary = CreateDictionary("the quick brown fox jumps over the lazy dog");

            ExercisePhrase(dictionary, "the quick brown fox jumps over the lazy dog the", "lazy dog");
        }

        [TestMethod]
        public void LazyPhraseCreator2()
        {
            var dictionary = CreateDictionary("the quick brown fox jumps over the lazy dog");

            ExercisePhrase(dictionary, "the quick brown fox jumps over the lazy dog the lazy", "dog");
        }

        [TestMethod]
        public void LazyPhraseCreator3()
        {
            var dictionary = CreateDictionary("the quick brown fox jumps over the lazy dog");

            ExercisePhrase(dictionary, "the quick brown fox jumps over the lazy dog the lazy lazy", "dog");
        }

        [TestMethod]
        public void ALazyDog()
        {
            var dictionary = CreateDictionary("the quick brown fox jumps over the lazy dog");

            ExercisePhrase(dictionary, "a lazy", "dog");
        }

        [TestMethod]
        public void ZLazyDog()
        {
            var dictionary = CreateDictionary("the quick brown fox jumps over the lazy dog");

            ExercisePhrase(dictionary, "z lazy", "dog");
        }

        [TestMethod]
        public void SaveAndLoadDictionary()
        {
            var originalDictionary = CreateDictionary();

            var stream = new MemoryStream();

            originalDictionary.Save(stream);

            stream.Position = 0;

            var reconstitutedDictionary = PredictionDictionary.Load(stream);

            Assert.AreEqual(stream.Length, stream.Position);

            ExercisePhrase(originalDictionary, "the quick brown", "fox jumped");
            ExercisePhrase(reconstitutedDictionary, "the quick brown", "fox jumped");
        }
    }
}
