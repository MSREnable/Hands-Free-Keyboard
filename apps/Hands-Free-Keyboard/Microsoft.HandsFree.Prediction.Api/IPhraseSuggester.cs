using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Api
{
    public interface IPhraseSuggester
    {
        IEnumerable<string> GetPhraseSuggestion(string[] previousWords);
    }
}
