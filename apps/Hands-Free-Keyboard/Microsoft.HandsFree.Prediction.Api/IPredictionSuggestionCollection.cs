namespace Microsoft.HandsFree.Prediction.Api
{
    using System.Collections.Generic;

    public interface IPredictionSuggestionCollection : IEnumerable<IPredictionSuggestion>
    {
        string[] Context { get; }
    }
}
