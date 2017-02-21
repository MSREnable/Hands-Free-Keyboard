namespace Microsoft.HandsFree.Prediction.Engine
{
    using Microsoft.HandsFree.Prediction.Api;
    using System.Collections.Generic;

    class NullWordSuggester : IWordSuggester
    {
        static string[] empty = new string[0];

        internal static readonly IWordSuggester Instance = new NullWordSuggester();

        NullWordSuggester()
        {
        }

        public IEnumerable<string> GetSuggestions(string[] previousWords, string currentWordPrefix)
        {
            return empty;
        }
    }
}
