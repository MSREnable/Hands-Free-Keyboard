using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Engine.Novelty;
using System;
using System.Linq;
using Microsoft.HandsFree.Settings.Nudgers;

namespace Microsoft.HandsFree.Prediction.Engine
{
    public static class PredictionEngineFactory
    {
        class PredictorDynamicValueSetting : DynamicValueSetting
        {
            internal PredictorDynamicValueSetting(string key, string valueString, Action<Predictor> updater)
            {
                Key = key;
                ValueString = valueString;
                Updater = updater;
            }

            public Action<Predictor> Updater { get; private set; }
        }

        static readonly PredictorDynamicValueSetting[] Predictors =
            {
                new PredictorDynamicValueSetting("Default", "Default (Layered)", (p) => LayeredPredictorFactory.UpdatePredictor(p)),
                new PredictorDynamicValueSetting("Layered", "Layered", (p) => LayeredPredictorFactory.UpdatePredictor(p)),
                new PredictorDynamicValueSetting("None", "None", (p) => SetNullPredictor(p))
            };

        static void UpdatePredictor(Predictor predictor)
        {
            var predictorName = predictor.Environment.PredictionSettings.Predictor;
            var setting = Predictors.FirstOrDefault((s) => s.Key == predictorName) ?? Predictors[0];
            setting.Updater(predictor);
        }

        static IPredictionEnvironment environmentConfigured;

        static void SetNullPredictor(Predictor predictor)
        {
            predictor.UpdateConfiguration(NullWordSuggester.Instance, NullPhraseSuggester.Instance);
        }

        public static IPredictor Create(IPredictionEnvironment environment)
        {
            LayeredPredictorFactory.Reset();

            if (environment != environmentConfigured)
            {
                environmentConfigured = environment;

                environment.PredictionSettings.PredictorNudger.Values = Predictors;
                environment.PredictionSettings.PropertyChanged += (s, e) => environment.PredictionSettings.PredictorNudger.Values = Predictors;
            }

            var innerPredictor = new Predictor(environment, NullWordSuggester.Instance, NullPhraseSuggester.Instance, UpdatePredictor);

            innerPredictor.QueueUpdate();

            var middlePredictor = new NoveltyWordPredictor(innerPredictor);

            var outerPredictor = new AmbiguousWordPredictor(middlePredictor);

            return outerPredictor;
        }
    }
}
