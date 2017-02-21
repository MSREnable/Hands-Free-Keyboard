namespace Microsoft.HandsFree.Prediction.Engine
{
    using Microsoft.HandsFree.Prediction.Api;
    using System.Collections.Generic;

    class NullPhraseSuggester : IPhraseSuggester
    {
        static string[] empty = new string[0];

        internal static readonly IPhraseSuggester Instance = new NullPhraseSuggester();

        NullPhraseSuggester()
        {
        }

        public IEnumerable<string> GetPhraseSuggestion(string[] previousWords)
        {
            return empty;
        }
    }
}
