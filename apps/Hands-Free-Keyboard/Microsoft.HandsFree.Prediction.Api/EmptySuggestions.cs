using System.Collections;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Api
{
    public class EmptySuggestions : IPredictionSuggestionCollection
    {
        internal static readonly IEnumerator<IPredictionSuggestion> emptyEnumerator = (new List<IPredictionSuggestion>()).GetEnumerator();

        public static readonly EmptySuggestions Instance = new EmptySuggestions();

        EmptySuggestions()
        {
        }

        public string[] Context { get { return new string[0]; } }

        public IEnumerator<IPredictionSuggestion> GetEnumerator()
        {
            return emptyEnumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return emptyEnumerator;
        }
    }
}
