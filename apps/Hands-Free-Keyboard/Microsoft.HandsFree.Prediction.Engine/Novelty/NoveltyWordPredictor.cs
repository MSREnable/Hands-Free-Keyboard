using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Prediction.Engine.Novelty
{
    using Microsoft.HandsFree.Prediction.Api;
    using Settings;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    class NoveltyWordPredictor : IPredictor
    {
        readonly IPredictor innerPredictor;

        readonly List<string> offeredList = new List<string>();

        internal NoveltyWordPredictor(IPredictor innerPredictor)
        {
            this.innerPredictor = innerPredictor;

            innerPredictor.PredictionChanged += (s, e) =>
                {
                    var handler = PredictionChanged;
                    if (handler != null)
                    {
                        handler(this, e);
                    }
                };
        }

        internal void UpdateOfferedList(IPredictionSuggestionCollection collection)
        {
            var fiddleFactor = AppSettings.Instance.Prediction.PredictionNovelty == PredictionNovelty.FromFirstLetter ? 0 : 1;

            using (var enumerator = collection.GetEnumerator())
            {
                var foundNovelty = false;
                while (!foundNovelty && enumerator.MoveNext())
                {
                    var candidate = enumerator.Current;
                    var offeredLimit = Math.Min(offeredList.Count, candidate.ReplacementLength + fiddleFactor);

                    var matchPosition = 0;
                    while (matchPosition < offeredLimit && offeredList[matchPosition] != candidate.Text)
                    {
                        matchPosition++;
                    }

                    foundNovelty = matchPosition == offeredLimit;

                    if (foundNovelty && candidate.ReplacementLength + fiddleFactor != 0)
                    {
                        while (offeredList.Count < candidate.ReplacementLength + fiddleFactor)
                        {
                            offeredList.Add(null);
                        }

                        offeredList[candidate.ReplacementLength + fiddleFactor - 1] = candidate.Text;

                        var builder = new StringBuilder();
                        for (var i = 0; i < candidate.ReplacementLength + fiddleFactor; i++)
                        {
                            builder.AppendFormat(" - {0}", offeredList[i]);
                        }
                        Debug.WriteLine(builder);
                    }
                }
            }
        }

        public IPrediction CreatePrediction(string text, int selectionStart, int selectionLength, bool isAutoSpace, object hints)
        {
            var innerPrediction = innerPredictor.CreatePrediction(text, selectionStart, selectionLength, isAutoSpace, hints);
            var prediction = new NoveltyWordPrediction(this, innerPrediction);
            return prediction;
        }

        public void RecordHistory(string text, bool isInPrivate)
        {
            innerPredictor.RecordHistory(text, isInPrivate);
        }

        public event EventHandler PredictionChanged;

        internal bool NoveltyFunction(IPredictionSuggestion suggestion)
        {
            var fiddleFactor = AppSettings.Instance.Prediction.PredictionNovelty == PredictionNovelty.FromFirstLetter ? 0 : 1;

            var matchIndex = 0;
            var matchLim = Math.Min(suggestion.ReplacementLength + fiddleFactor - 1, offeredList.Count);

            while (matchIndex < matchLim && offeredList[matchIndex] != suggestion.Text)
            {
                matchIndex++;
            }

            return matchIndex == matchLim;
        }
    }
}

