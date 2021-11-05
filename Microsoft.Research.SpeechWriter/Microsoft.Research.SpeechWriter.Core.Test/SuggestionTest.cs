using Microsoft.Research.SpeechWriter.Core.Items;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    [Parallelizable(ParallelScope.All)]
    public class SuggestionTest
    {
        private class SeededEnvironment : DefaultWriterEnvironment, IWriterEnvironment
        {
            private readonly string[] _seeds;

            internal SeededEnvironment(params string[] seeds)
            {
                _seeds = seeds;
            }

            /// <summary>
            /// Dictionary of words, listed from most likely to least likely.
            /// </summary>
            /// <returns>List of words.</returns>
            public IEnumerable<string> GetOrderedSeedWords()
            {
                return _seeds;
            }
        }

        private static ApplicationModel CreateModel(params string[] seeds)
        {
            var environment = new SeededEnvironment(seeds);
            var model = new ApplicationModel(environment);
            return model;
        }

        [Test]
        public void EmptyWords()
        {
            var model = CreateModel();

            Assert.AreEqual(1, model.SuggestionLists.Count);

            Assert.AreEqual(2, model.SuggestionInterstitials.Count);
            Assert.IsInstanceOf<InterstitialSpellingItem>(model.SuggestionInterstitials[1]);
        }

        [Test]
        public void LonelyX()
        {
            var model = CreateModel("X");

            Assert.AreEqual(2, model.SuggestionLists.Count);
            Assert.AreEqual(1, model.SuggestionLists[1].Count());
            var tile = model.SuggestionLists[1].First();
            Assert.IsInstanceOf<SuggestedWordItem>(tile);
            var suggestedWordTile = (SuggestedWordItem)tile;
            Assert.AreEqual("X", suggestedWordTile.FormattedContent);

            Assert.AreEqual(3, model.SuggestionInterstitials.Count);
            Assert.IsInstanceOf<InterstitialNonItem>(model.SuggestionInterstitials[0]);
            Assert.IsInstanceOf<InterstitialSpellingItem>(model.SuggestionInterstitials[1]);
            Assert.IsInstanceOf<InterstitialSpellingItem>(model.SuggestionInterstitials[2]);
        }

        [Test]
        public void TwoXs()
        {
            var model = CreateModel("X", "x");

            Assert.AreEqual(2, model.SuggestionLists.Count);
            Assert.AreEqual(1, model.SuggestionLists[1].Count());
            var tile = model.SuggestionLists[1].First();
            Assert.IsInstanceOf<SuggestedWordItem>(tile);
            var suggestedWordTile = (SuggestedWordItem)tile;
            Assert.AreEqual("X", suggestedWordTile.FormattedContent);

            Assert.AreEqual(3, model.SuggestionInterstitials.Count);
            Assert.IsInstanceOf<InterstitialNonItem>(model.SuggestionInterstitials[0]);
            Assert.IsInstanceOf<InterstitialSpellingItem>(model.SuggestionInterstitials[1]);
            Assert.IsInstanceOf<InterstitialSpellingItem>(model.SuggestionInterstitials[2]);
        }
    }
}
