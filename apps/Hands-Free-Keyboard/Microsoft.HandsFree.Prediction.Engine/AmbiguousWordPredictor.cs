using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Engine.Novelty;
using System;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Engine
{
    internal class AmbiguousWordPredictor : IPredictor
    {
        readonly NoveltyWordPredictor _innerPredictor;

        public AmbiguousWordPredictor(NoveltyWordPredictor innerPredictor)
        {
            _innerPredictor = innerPredictor;
        }

        event EventHandler IPredictor.PredictionChanged
        {
            add
            {
                _innerPredictor.PredictionChanged += value; ;
            }

            remove
            {
                _innerPredictor.PredictionChanged -= value;
            }
        }

        IPrediction IPredictor.CreatePrediction(string text, int selectionStart, int selectionLength, bool isAutoSpace, object hints)
        {
            IPrediction prediction;

            if (hints != null)
            {
                var clusterSequence = (List<List<string>>)hints;
                prediction = new AmbiguousWordPrediction(text, selectionStart, selectionLength, isAutoSpace, clusterSequence);
            }
            else
            {
                prediction = _innerPredictor.CreatePrediction(text, selectionStart, selectionLength, isAutoSpace, hints);
            }

            return prediction;
        }

        void IPredictor.RecordHistory(string text, bool isInPrivate)
        {
            _innerPredictor.RecordHistory(text, isInPrivate);
        }
    }
}
