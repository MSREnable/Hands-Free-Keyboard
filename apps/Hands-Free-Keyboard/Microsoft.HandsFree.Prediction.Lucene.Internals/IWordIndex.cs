using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Lucene.Internals
{
    public interface IWordIndex
    {
        IEnumerable<string> Query(string queryString);
    }
}
