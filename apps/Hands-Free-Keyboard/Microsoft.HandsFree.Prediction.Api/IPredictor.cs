namespace Microsoft.HandsFree.Prediction.Api
{
    using System;
    using System.Threading.Tasks;

    public interface IPredictor
    {
        IPrediction CreatePrediction(string text,
            int selectionStart,
            int selectionLength,
            bool isAutoSpace,
            object hints);

        void RecordHistory(string text, bool isInPrivate);

        event EventHandler PredictionChanged;
    }
}

