using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.HandsFree.Prediction.Lucene.Internals;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.HandsFree.Prediction.Lucene
{
    public class WordIndex : IWordIndex
    {
        internal const string LeadCharacter = ".";
        internal const string TailCharacter = ",";
        internal const string WordFieldName = "word";

        readonly Analyzer analyzer;
        readonly Directory index;

        internal WordIndex(Analyzer analyzer, Directory index)
        {
            this.analyzer = analyzer;
            this.index = index;
        }

        internal static Analyzer CreateAnalyzer()
        {
            return new NGramAnalyzer();
        }

        public IEnumerable<string> Query(string queryString)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(LeadCharacter) | !string.IsNullOrWhiteSpace(queryString));

            var parser = new QueryParser(Version.LUCENE_30, WordFieldName, analyzer);

            var query = new BooleanQuery();

            var parsed = parser.Parse(LeadCharacter + queryString);

            var extractedTerms = new HashSet<Term>();
            parsed.ExtractTerms(extractedTerms);
            foreach (var term in extractedTerms)
            {
                query.Add(new TermQuery(term), Occur.SHOULD);
            }

            var hitsPerPage = 20 + queryString.Length;
            using (var reader = IndexReader.Open(index, true))
            {
                using (var searcher = new IndexSearcher(reader))
                {
                    var collector = TopScoreDocCollector.Create(hitsPerPage, true);
                    searcher.Search(query, collector);
                    var hits = collector.TopDocs();

                    foreach (var scoreDoc in hits.ScoreDocs)
                    {
                        var docId = scoreDoc.Doc;
                        var document = searcher.Doc(docId);

                        var adornedWord = document.GetField(WordFieldName).StringValue;

                        var suggestion = adornedWord.Substring(LeadCharacter.Length, adornedWord.Length - LeadCharacter.Length - TailCharacter.Length);

                        //if (!queryString.StartsWith(suggestion))
                        {
                            yield return suggestion;
                        }
                    }
                }
            }
        }
    }
}
