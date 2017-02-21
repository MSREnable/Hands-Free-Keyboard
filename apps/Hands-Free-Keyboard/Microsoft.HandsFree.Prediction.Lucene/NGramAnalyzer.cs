using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using System.IO;

namespace Microsoft.HandsFree.Prediction.Lucene
{
    class NGramAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string str, TextReader r)
        {
            var source = new NGramTokenizer(r, 3, 4);
            return source;
        }
    }
}
