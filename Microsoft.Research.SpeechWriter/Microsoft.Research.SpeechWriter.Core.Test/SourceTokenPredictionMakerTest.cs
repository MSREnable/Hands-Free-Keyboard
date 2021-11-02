using Microsoft.Research.SpeechWriter.Core.Items;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    public class SourceTokenPredictionMakerTest
    {
        private const int PredictorWidth = 4;

        class TestVocabularySource : PredictiveVocabularySource<SuggestedWordItem>, ITokenTileFilter
        {
            internal TestVocabularySource(ApplicationModel model)
                : base(model, PredictorWidth)
            {

            }

            internal TokenPredictor Predictor => PersistantPredictor;

            internal override ITokenTileFilter TokenFilter => this;

            internal override int Count => 4;

            internal override SuggestedWordItem GetIndexItem(int index)
            {
                throw new NotImplementedException();
            }

            internal override int GetIndexToken(int index)
            {
                return index + 1;
            }

            internal override int GetTokenIndex(int token)
            {
                return token - 1;
            }

            internal override IEnumerable<int> GetTokens()
            {
                for (var i = Count; 0 < i; i--)
                {
                    yield return i;
                }
            }

            bool ITileFilter.IsIndexVisible(int index) => true;

            bool ITokenTileFilter.IsTokenVisible(int token) => true;
        }

        [Test]
        public void TestScoring()
        {
            var model = new ApplicationModel();
            var source = new TestVocabularySource(model);
            source.Predictor.AddSequence(new[] { 0, 1, 2, 3 }, 1);
            source.Predictor.AddSequence(new[] { 0, 4, 2, 3 }, 2);

            var predictor = source.Predictor;
            var scores = predictor.GetTopScores(source, source, new int[] { 0, 1, 2 }, 0, int.MaxValue);

            using (var enumerator = scores.GetEnumerator())
            {
                CheckStep(3, 3, 3, 1, 1);
                CheckStep(2, 3);
                CheckStep(4, 2);
                CheckStep(1, 1);

                Assert.IsFalse(enumerator.MoveNext());

                void CheckStep(params int[] expecteds)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    var actuals = enumerator.Current;

                    Assert.AreEqual(expecteds.Length, actuals.Length);
                    for (var i = 0; i < expecteds.Length; i++)
                    {
                        Assert.AreEqual(expecteds[i], actuals[i]);
                    }
                }
            }
            foreach (var row in scores)
            {
                var score = string.Join("-", row);
                Debug.WriteLine(score);
            }
        }
    }
}
