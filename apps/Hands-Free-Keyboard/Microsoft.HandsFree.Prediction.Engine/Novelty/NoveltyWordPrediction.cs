using Microsoft.HandsFree.Prediction.Api;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Prediction.Engine.Novelty
{
    class NoveltyWordPrediction : IPrediction
    {
        readonly NoveltyWordPredictor predictor;
        readonly IPrediction innerPrediction;

        string[] phraseContext;

        internal NoveltyWordPrediction(NoveltyWordPredictor predictor, IPrediction innerPrediction)
        {
            this.predictor = predictor;
            this.innerPrediction = innerPrediction;
        }

        public IPredictionSuggestionCollection GetSuggestions(SuggestionType type)
        {
            IPredictionSuggestionCollection outerCollection;

            switch (type)
            {
                case SuggestionType.Word:
                    var innerCollection = innerPrediction.GetSuggestions(type);
                    predictor.UpdateOfferedList(innerCollection);
                    outerCollection = new NoveltyWordPredictionSuggestionCollection(predictor, innerCollection);

                    var phraseContextList = new List<string>();

                    using (var enumerator = outerCollection.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            phraseContextList.Add(enumerator.Current.Text);
                        }
                    }

                    foreach (var word in outerCollection.Context)
                    {
                        phraseContextList.Add(word);
                    }

                    phraseContext = phraseContextList.ToArray();

                    break;

                case SuggestionType.Phrase:
                    var prediction = (Prediction)innerPrediction;
                    outerCollection = prediction.GetPhraseSuggestion(phraseContext);
                    break;

                default:
                    outerCollection = innerPrediction.GetSuggestions(type);
                    break;
            }

            return outerCollection;
        }
    }
}
