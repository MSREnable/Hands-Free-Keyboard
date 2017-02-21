namespace Microsoft.HandsFree.Prediction.Engine
{
    using Microsoft.HandsFree.Prediction.Api;
    using System;
    using System.Collections.Generic;

    class CompoundWordSuggester : IWordSuggester
    {
        readonly IWordSuggester firstSuggester;
        readonly IWordSuggester secondSuggester;

        internal CompoundWordSuggester(IWordSuggester firstSuggester, IWordSuggester secondSuggester)
        {
            this.firstSuggester = firstSuggester;
            this.secondSuggester = secondSuggester;
        }

        public IEnumerable<string> GetSuggestions(string[] previousWords, string currentWordPrefix)
        {
            var suggested = new HashSet<string>();

            var lowercaseCurrentWordPrefx = currentWordPrefix.ToLowerInvariant();

            var firstSuggestions = firstSuggester.GetSuggestions(previousWords, lowercaseCurrentWordPrefx);
            foreach (var s in firstSuggestions)
            {
                yield return s;

                suggested.Add(s);
            }

            var secondSuggestions = secondSuggester.GetSuggestions(previousWords, lowercaseCurrentWordPrefx);
            foreach (var s in secondSuggestions)
            {
                if (!suggested.Contains(s))
                {
                    yield return s;
                }
            }
        }
    }
}
