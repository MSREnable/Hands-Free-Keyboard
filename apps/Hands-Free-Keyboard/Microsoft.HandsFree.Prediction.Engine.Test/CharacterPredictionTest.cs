namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    using Microsoft.HandsFree.LanguageHelpers;
    using Microsoft.HandsFree.Prediction.Api;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;

    [TestClass]
    public class CharacterPredictionTest
    {
        [TestMethod]
        public void SimpleCreation()
        {
            var predictor = PredictionEngineFactory.Create(new SingleThreadedPredictionEnvironment());
            Assert.IsNotNull(predictor);
        }

        [TestMethod]
        public void FirstPrediction()
        {
            var predictor = PredictionEngineFactory.Create(new SingleThreadedPredictionEnvironment());
            var prediction = predictor.CreatePrediction("", 0, 0, false, null);
            var suggestions = prediction.GetSuggestions(SuggestionType.Character);

            var count = suggestions.Count();
            Assert.AreEqual(26, count, "Expect to get 26 lowercase letter suggestions");

            var suggestionT = (from s in suggestions where s.Text == "t" select s).First();
            var suggestionZ = (from s in suggestions where s.Text == "z" select s).First();

            Assert.IsTrue(suggestionZ.Confidence < suggestionT.Confidence);
        }

        [TestMethod]
        public void CoverCharacterPrediction()
        {
            var validChar = ProbabilityTable.GetProbability(1, 'a', 'b');
            Assert.AreNotEqual(0, validChar);

            var invalidChar = ProbabilityTable.GetProbability(0, '+', '¬');
            Assert.AreEqual(0, invalidChar);

            var tooLongWord = ProbabilityTable.GetProbability(128, 'a', 'b');
            Assert.AreEqual(0, tooLongWord);
        }

        [TestMethod]
        public void FirstAndSecondPredictionsDifferent()
        {
            var predictor = PredictionEngineFactory.Create(new SingleThreadedPredictionEnvironment());

            var prediction0 = predictor.CreatePrediction("", 0, 0, false, null);
            var suggestions0 = prediction0.GetSuggestions(SuggestionType.Character);

            var predictionT = predictor.CreatePrediction("T", 1, 0, false, null);
            var suggestionsT = predictionT.GetSuggestions(SuggestionType.Character);

            var sameProbabilityCount = 0;
            var similarProbabilityCount = 0;

            using (var enumerator0 = suggestions0.GetEnumerator())
            {
                foreach (var suggestionT in suggestionsT)
                {
                    Assert.IsTrue(enumerator0.MoveNext());
                    var suggestion0 = enumerator0.Current;

                    var probabilityDifference = suggestion0.Confidence - suggestionT.Confidence;

                    if (Math.Abs(probabilityDifference) < 0.01)
                    {
                        similarProbabilityCount++;

                        Assert.IsFalse(probabilityDifference == 0, "Differences should not be absolutely zero");
                    }
                }

                Assert.IsFalse(enumerator0.MoveNext());

                Assert.IsTrue(similarProbabilityCount < 13);
                Assert.IsTrue(sameProbabilityCount == 0);
            }
        }
    }
}
