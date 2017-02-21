using Microsoft.HandsFree.Prediction.Test;
using System;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Measurer
{
    class SingleThreadedPredictionEnvironment : TestPredictionEnvironment
    {
        internal List<Action> workItems = new List<Action>();

        internal SingleThreadedPredictionEnvironment()
        {
            PredictionSettings.Predictor = "Layered";
        }

        public override void QueueWorkItem(Action action)
        {
            workItems.Add(action);
        }
    }
}
