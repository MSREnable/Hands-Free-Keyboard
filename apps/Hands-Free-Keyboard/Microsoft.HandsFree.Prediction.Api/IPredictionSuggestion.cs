namespace Microsoft.HandsFree.Prediction.Api
{
    using System.Threading.Tasks;

    public interface IPredictionSuggestion
    {
        double Confidence { get; }

        string Text { get; }

        int ReplacementStart { get; }

        int ReplacementLength { get; }

        bool CompleteWord { get; }

        void Accepted(int index);
    }
}

