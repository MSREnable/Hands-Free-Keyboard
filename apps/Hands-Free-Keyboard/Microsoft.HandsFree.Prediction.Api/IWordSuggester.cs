namespace Microsoft.HandsFree.Prediction.Api
{
    using System.Collections.Generic;

    public interface IWordSuggester
    {
        IEnumerable<string> GetSuggestions(string[] previousWords, string currentWordPrefix);
    }
}
