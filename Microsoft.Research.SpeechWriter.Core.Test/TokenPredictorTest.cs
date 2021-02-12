using NUnit.Framework;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    public class TokenPredictorTest
    {
        [Test]
        public void BuildSeedPredictor()
        {
            var tokens = new StringTokens();
            var predictor1 = new TokenPredictor(3);
            var predictor2 = new TokenPredictor(3);
            var predictor3 = new TokenPredictor(3);

            var sentenceCount = 0;
            var wordCount = 0;
            foreach (var sentence in DefaultWriterEnvironment.Instance.GetSeedSentences())
            {
                sentenceCount++;

                var sentenceSoFar = new List<int>();

                foreach (var word in sentence)
                {
                    wordCount++;
                    var token = tokens.GetToken(word);
                    sentenceSoFar.Add(token);

                    predictor1.AddSequenceTail(sentenceSoFar, 1);
                    predictor2.AddSequenceTail(sentenceSoFar, 2);
                    predictor3.AddSequenceTail(sentenceSoFar, 3);
                }
            }

            var predictor1plus2 = predictor1.CreateCopy();
            CheckJsonEquivalent(tokens, predictor1plus2, predictor1);
            predictor1plus2.Add(predictor2);
            CheckJsonEquivalent(tokens, predictor1plus2, predictor3);

            var predictor3minus2 = predictor3.CreateCopy();
            CheckJsonEquivalent(tokens, predictor3minus2, predictor3);
            predictor3minus2.Subtract(predictor2);
            CheckJsonEquivalent(tokens, predictor3minus2, predictor1);

            predictor3minus2.Subtract(predictor1);
            var predictor0 = predictor1.CreateEmpty();
            CheckJsonEquivalent(tokens, predictor3minus2, predictor0);
        }

        private static void CheckJsonEquivalent(StringTokens tokens, TokenPredictor l, TokenPredictor r)
        {
            var lString = l.ToJson(tokens);
            var rString = r.ToJson(tokens);
            Assert.AreEqual(lString, rString);
        }

        private const string AlphabetJson = "{\"\":{\"#\":1,\"~\":{\"A\":{\"#\":1,\"~\":{\"B\":{\"#\":1}}}}},\"A\":{\"#\":1,\"~\":{\"B\":{\"#\":1,\"~\":{\"C\":{\"#\":1}}}}},\"B\":{\"#\":1,\"~\":{\"C\":{\"#\":1,\"~\":{\"D\":{\"#\":1}}}}},\"C\":{\"#\":1,\"~\":{\"D\":{\"#\":1,\"~\":{\"E\":{\"#\":1}}}}},\"D\":{\"#\":1,\"~\":{\"E\":{\"#\":1,\"~\":{\"F\":{\"#\":1}}}}},\"E\":{\"#\":1,\"~\":{\"F\":{\"#\":1,\"~\":{\"G\":{\"#\":1}}}}},\"F\":{\"#\":1,\"~\":{\"G\":{\"#\":1,\"~\":{\"H\":{\"#\":1}}}}},\"G\":{\"#\":1,\"~\":{\"H\":{\"#\":1}}},\"H\":{\"#\":1}}";

        private static List<int> GetAlphabet(StringTokens tokens)
        {
            var alphabet = new List<int>();
            alphabet.Add(0);

            for (var letter = 'A'; letter <= 'H'; letter++)
            {
                var token = tokens.GetToken(letter.ToString());
                alphabet.Add(token);
            }

            return alphabet;
        }

        [Test]
        public void CreateAlphabetTest()
        {
            var tokens = new StringTokens();
            var predictor = new TokenPredictor(3);

            List<int> alphabet = GetAlphabet(tokens);

            predictor.AddSequence(alphabet, 1);

            var json = predictor.ToJson(tokens);
            Assert.AreEqual(AlphabetJson, json);
        }

        [Test]
        public void CreateAlphabetLetterByLetterTest()
        {
            var tokens = new StringTokens();
            var predictor = new TokenPredictor(3);

            var alphabet = GetAlphabet(tokens);

            var incrementalAlphabet = new List<int>();
            foreach (var token in alphabet)
            {
                incrementalAlphabet.Add(token);
                predictor.AddSequenceTail(incrementalAlphabet, 1);
            }

            var json = predictor.ToJson(tokens);
            Assert.AreEqual(AlphabetJson, json);
        }
    }
}
