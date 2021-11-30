using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    class NascentWordPredictionList
    {
        internal WordPrediction _first;
        internal readonly List<WordPrediction> _list;
        internal WordPrediction _last;

        internal WordPrediction _followOn;

        internal NascentWordPredictionList(WordPrediction prediction, WordPrediction followOn)
        {
            _first = prediction;
            _list = new List<WordPrediction>(1) { prediction };
            _last = prediction;

            _followOn = followOn;
        }

        internal bool CanMergeWithNext(NascentWordPredictionList next)
        {
            bool canMerge;

            if (next._first.Text.StartsWith(_first.Text))
            {
                var prevCount = _list.Count;
                var prevTailPrediction = _list[prevCount - 1];
                var prevTailText = prevTailPrediction.Text;

                var nextCount = next._list.Count;
                var nextTailPrediction = next._list[nextCount - 1];
                var nextTailText = nextTailPrediction.Text;

                canMerge = nextTailText.StartsWith(prevTailText) || prevTailText.StartsWith(nextTailText);
            }
            else
            {
                canMerge = false;
            }

            return canMerge;
        }

        internal void MergeWithNext(NascentWordPredictionList next)
        {
            var targetPredictions = _list;
            var sourcePredictions = next._list;

            Debug.Assert(targetPredictions[0].Text.Length < sourcePredictions[0].Text.Length);
            Debug.Assert(sourcePredictions[0].Text.StartsWith(targetPredictions[0].Text));

            var sourcePosition = 0;
            var targetPosition = 0;
            while (sourcePosition < sourcePredictions.Count && targetPosition < targetPredictions.Count)
            {
                if (sourcePredictions[sourcePosition].Index < targetPredictions[targetPosition].Index)
                {
                    Debug.Assert(targetPredictions[targetPosition].Text.StartsWith(sourcePredictions[sourcePosition].Text));
                    targetPredictions.Insert(targetPosition, sourcePredictions[sourcePosition]);
                    sourcePosition++;
                }
                else
                {
                    Debug.Assert(targetPredictions[targetPosition].Index < sourcePredictions[sourcePosition].Index);
                    Debug.Assert(sourcePredictions[sourcePosition].Text.StartsWith(targetPredictions[targetPosition].Text));
                }
                targetPosition++;
            }

            if (sourcePosition < sourcePredictions.Count)
            {
                while (sourcePosition < sourcePredictions.Count)
                {
                    targetPredictions.Add(sourcePredictions[sourcePosition]);
                    sourcePosition++;
                }

                _followOn = next._followOn;
            }
            else
            {
                Debug.WriteLine("TODO: Debug.Assert(ReferenceEquals(_followOn, next._followOn));");
            }

            Debug.Assert(ReferenceEquals(_first, _list[0]));
        }
    }
}
