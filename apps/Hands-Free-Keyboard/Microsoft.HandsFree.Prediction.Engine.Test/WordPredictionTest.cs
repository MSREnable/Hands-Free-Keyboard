namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    using Microsoft.HandsFree.Prediction.Api;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class WordPredictionTest
    {
        [TestMethod]
        public void SimpleCreation()
        {
            var predictor = PredictionEngineFactory.Create(new SingleThreadedPredictionEnvironment());
            Assert.IsNotNull(predictor);
        }

        [TestMethod]
        public void ComplexCreation()
        {
            var environment = new SingleThreadedPredictionEnvironment(true);
            var queue = new List<Action>();

            var predictor = PredictionEngineFactory.Create(environment);
            Assert.IsNotNull(predictor, "We got a predictor");

            Assert.AreEqual(1, environment.workItems.Count, "Queue contains the refresh action");

            var updated = false;
            predictor.PredictionChanged += (s, e) => updated = true;
            environment.workItems[0]();
            Assert.IsTrue(updated);
            environment.workItems.RemoveAt(0);

            Assert.AreEqual(0, environment.workItems.Count, "Queue is now empty");
        }

        void CheckPrediction(IPredictor predictor, string root, params string[] expectations)
        {
            var prediction = predictor.CreatePrediction(root, root.Length, 0, false, null);
            Assert.IsNotNull(prediction, "Should get a prediction");

            var collection = prediction.GetSuggestions(SuggestionType.Word);
            Assert.IsNotNull(collection, "Should get a collection");

            using (var enumerator = collection.GetEnumerator())
            {
                for (var i = 0; i < expectations.Length; i++)
                {
                    Assert.IsTrue(enumerator.MoveNext(), "Should be another suggestion");
                    Assert.AreEqual(expectations[i], enumerator.Current.Text, "Suggestions should match");
                }

                // Run enumerator to exhaustion to get code coverage.
                while (enumerator.MoveNext())
                {
                    // Spin.
                }

                if (expectations.Length != 0)
                {
                    var primeSuggestion = collection.First();
                    primeSuggestion.Accepted(0);
                }
            }
        }

        [TestMethod]
        public void PredictionWalkThrough()
        {
            var environment = new SingleThreadedPredictionEnvironment
            {
                History = "the them they them they the the the" + Environment.NewLine
            };

            var predictor = PredictionEngineFactory.Create(environment);
            Assert.IsNotNull(predictor, "We got a predictor");

            CheckPrediction(predictor, "");

            Assert.AreEqual(1, environment.workItems.Count, "Queue contains the refresh action");
            environment.workItems[0]();
            environment.workItems.RemoveAt(0);
            Assert.AreEqual(0, environment.workItems.Count, "Queue is now empty");

            CheckPrediction(predictor, "", "the", "they", "them");

            Assert.AreEqual(0, environment.workItems.Count, "Queue is still empty");

            CheckPrediction(predictor, "t", "the", "they", "them");
            CheckPrediction(predictor, "th", "they", "the", "them");
            CheckPrediction(predictor, "the", "them", "the", "they");

            predictor.RecordHistory("Wibble", false);
            Assert.AreEqual("the them they them they the the the" + Environment.NewLine + "Wibble" + Environment.NewLine, environment.History, "History should have updated");
        }
    }
}
