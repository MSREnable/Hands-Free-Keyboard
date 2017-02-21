using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Lucene.Internals;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Engine
{
    class LuceneWordSuggester : IWordSuggester
    {
        readonly IWordIndex index;

        internal LuceneWordSuggester(IWordIndex index)
        {
            this.index = index;
        }

        public IEnumerable<string> GetSuggestions(string[] previousWords, string currentWordPrefix)
        {
            var lowercaseWordPrefix = currentWordPrefix.ToLowerInvariant();
            var enumerable = index.Query(lowercaseWordPrefix);
            return enumerable;
        }
    }
}
