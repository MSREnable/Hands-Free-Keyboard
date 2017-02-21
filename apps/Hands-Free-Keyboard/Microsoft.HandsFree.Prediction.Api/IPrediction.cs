namespace Microsoft.HandsFree.Prediction.Api
{
    using System.Threading.Tasks;

    public interface IPrediction
    {
        IPredictionSuggestionCollection GetSuggestions(SuggestionType type);
    }
}
