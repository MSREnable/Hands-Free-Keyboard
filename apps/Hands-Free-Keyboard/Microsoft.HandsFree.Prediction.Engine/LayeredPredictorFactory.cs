using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Historic;
using Microsoft.HandsFree.Prediction.Lucene;

namespace Microsoft.HandsFree.Prediction.Engine
{
    static class LayeredPredictorFactory
    {
        static LuceneWordSuggester _luceneWordSuggester;

        static PredictionDictionary _historicSuggester;

        internal static void Reset()
        {
            _historicSuggester = null;
        }

        static CompoundWordSuggester CreateCompoundWordSuggester(IWordSuggester simpleWordSuggester)
        {
            var selectingWordSuggester = new CompoundWordSuggester(simpleWordSuggester, _luceneWordSuggester);
            return selectingWordSuggester;
        }

        internal static void UpdatePredictor(Predictor predictor)
        {
            if (_luceneWordSuggester == null)
            {
                var index = WordIndexFactory.CreateFromWordCountList(predictor.Environment, WordScorePairEnumerable.Instance);
                _luceneWordSuggester = new LuceneWordSuggester(index);
            }

            if (_historicSuggester == null)
            {
                _historicSuggester = PredictionDictionary.Create(predictor.Environment);
            }

            var newHistory = predictor.ConsumeNewHistory();

            var updated = false;
            foreach (var utterance in newHistory)
            {
                updated = true;
                _historicSuggester.AddRawPhrases(utterance);
            }

            if (updated)
            {
                using (var stream = predictor.Environment.CreateDynamicDictionaryCache())
                {
                    _historicSuggester.Save(stream);
                }
            }

            var historicWithFallbackSuggester = new CompoundWordSuggester(_historicSuggester, SingleLetterSuggester.Instance);

            var wordSuggester = CreateCompoundWordSuggester(historicWithFallbackSuggester);

            predictor.UpdateConfiguration(wordSuggester, _historicSuggester);
        }
    }
}
