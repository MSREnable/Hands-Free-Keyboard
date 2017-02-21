using Microsoft.HandsFree.Prediction.Engine;
using Microsoft.HandsFree.Prediction.Historic.Test.Properties;
using Microsoft.HandsFree.Prediction.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Prediction.Historic.Test
{
    [TestClass]
    public class Stressful
    {
        [TestMethod]
        public void WarAndPeaceFireUp()
        {
            var environment = new TestPredictionEnvironment(Resources.spoken);
            var predictor = PredictionEngineFactory.Create(environment);
        }
    }
}
