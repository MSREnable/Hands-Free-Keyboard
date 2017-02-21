using Microsoft.HandsFree.Prediction.Test;
using System;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    class SingleThreadedPredictionEnvironment : TestPredictionEnvironment
    {
        internal List<Action> workItems = new List<Action>();

        internal SingleThreadedPredictionEnvironment(bool isLayered)
        {
            PredictionSettings.Predictor = isLayered ? "Layered" : null;
        }

        internal SingleThreadedPredictionEnvironment()
            : this(false)
        {
        }

        public override void QueueWorkItem(Action action)
        {
            workItems.Add(action);
        }
    }
}
