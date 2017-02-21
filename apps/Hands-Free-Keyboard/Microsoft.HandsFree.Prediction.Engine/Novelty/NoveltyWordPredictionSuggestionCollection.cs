namespace Microsoft.HandsFree.Prediction.Engine.Novelty
{
    using Microsoft.HandsFree.Prediction.Api;

    class NoveltyWordPredictionSuggestionCollection : NoveltyEnumeratable<IPredictionSuggestion>, IPredictionSuggestionCollection
    {
        readonly IPredictionSuggestionCollection innerCollection;

        internal NoveltyWordPredictionSuggestionCollection(NoveltyWordPredictor predictor, IPredictionSuggestionCollection innerCollection)
            : base(innerCollection, predictor.NoveltyFunction)
        {
            this.innerCollection = innerCollection;
        }

        public string[] Context { get { return innerCollection.Context; } }
    }
}
