using Microsoft.Research.SpeechWriter.Core.Automation;
using Microsoft.Research.SpeechWriter.Core.Items;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    public class ApplicationRobotTest
    {
        private static void CheckModelLinkage(ApplicationModel model)
        {
            ITile commonPredecessor;

            using (var enumerator = model.HeadItems.GetEnumerator())
            {
                Assert.IsTrue(enumerator.MoveNext());
                var predecessor = enumerator.Current;
                commonPredecessor = predecessor;
                Assert.IsNull(predecessor.Predecessor);

                while (enumerator.MoveNext())
                {
                    Assert.AreSame(predecessor, enumerator.Current.Predecessor);
                    if (enumerator.Current is HeadWordItem)
                    {
                        Assert.AreSame(commonPredecessor, predecessor);
                        commonPredecessor = enumerator.Current;
                    }
                    predecessor = enumerator.Current;
                }
            }

            foreach (var interstital in model.SuggestionInterstitials)
            {
                if (interstital is InterstitialNonItem)
                {
                    Assert.IsNull(interstital.Predecessor);
                }
                else
                {
                    Assert.AreSame(commonPredecessor, interstital.Predecessor);
                }
            }

            foreach (var list in model.SuggestionLists)
            {
                var predecessor = commonPredecessor;
                var count = 0;
                var spellingSeen = false;
                foreach (var suggestion in list)
                {
                    if (spellingSeen && suggestion is SuggestedWordItem)
                    {
                        Assert.AreSame(commonPredecessor, suggestion.Predecessor);
                    }
                    else
                    {
                        Assert.AreSame(predecessor, suggestion.Predecessor);

                        if (suggestion is SuggestedSpellingItem)
                        {
                            spellingSeen = true;
                        }
                    }
                    predecessor = suggestion;

                    count++;
                }
            }
        }

        private static int CountClicks(ApplicationModel model, string[] words, double errorRate)
        {
            var random = new Random(0);

            var count = 0;
            var errorCount = 0;

            var hasNotified = false;
            var expectedIsComplete = false;

            var actionIsComplete = false;
            var isGood = false;

            void OnNotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                Assert.IsFalse(hasNotified, "No changes after model update notification");
            }

            void OnApplicationModelUpdated(object sender, ApplicationModelUpdateEventArgs e)
            {
                Assert.IsFalse(hasNotified, "No previous model update notification");
                hasNotified = true;

                Assert.AreEqual(isGood && actionIsComplete, isGood && e.IsComplete);

                if (e.IsComplete)
                {
                    expectedIsComplete = true;
                }
                else
                {
                    expectedIsComplete = false;
                }
            }

            var collections = new INotifyCollectionChanged[] { model.HeadItems, model.TailItems, model.SuggestionInterstitials, model.SuggestionLists };
            foreach (var collection in collections)
            {
                collection.CollectionChanged += OnNotifyCollectionChanged;
            }
            model.ApplicationModelUpdate += OnApplicationModelUpdated;

            bool done;
            do
            {
                ApplicationRobotAction action;

                if (errorCount < 100 && random.NextDouble() < errorRate)
                {
                    isGood = false;
                    action = ApplicationRobot.GetRandom(model, random);
                    errorCount++;
                }
                else if (random.NextDouble() < errorRate / 20.0)
                {
                    isGood = false;
                    action = ApplicationRobot.GetRandom(model, random);
                    errorCount++;
                }
                else
                {
                    isGood = true;
                    action = ApplicationRobot.GetNextCompletionAction(model, words);
                }

                var item = action.GetItem(model);
                //if (item is InterstitialNonItem)
                {
                    Assert.IsNotInstanceOf<InterstitialNonItem>(item, "Should never click on a non-item");
                }

                hasNotified = false;
                actionIsComplete = action.IsComplete;
                action.ExecuteItem(model);
                Assert.IsTrue(hasNotified, "We should have seen a notification");
                Assert.AreEqual(isGood && expectedIsComplete, isGood && action.IsComplete, "IsComplete in action and notification should match");

                CheckModelLinkage(model);

                count++;
                done = isGood && action.IsComplete;

#if false
                if (9900 <= count)
                {
                    if (Debugger.IsAttached)
                    {
                        var item = action.GetItem(model);
                        Debug.WriteLine($"{count}: {isGood} {action.Target} {action.Index}.{action.SubIndex} - {item.GetType().Name} - {item}");
                    }

                    if (9950 <= count)
                    {
                        Assert.IsTrue(count < 10000, "Clicking ends in reasonable time");
                    }
                }
#else
                Assert.IsTrue(count < 10000, "Clicking ends in reasonable time");
#endif
            }
            while (!done);

            foreach (var collection in collections)
            {
                collection.CollectionChanged -= OnNotifyCollectionChanged;
            }
            model.ApplicationModelUpdate -= OnApplicationModelUpdated;


            // Check there is but one action needed to re-establish text.
            var reestablishAction = ApplicationRobot.GetNextEstablishingAction(model, words);
            Assert.IsNotNull(reestablishAction);
            Assert.IsFalse(reestablishAction.IsComplete);
            reestablishAction.ExecuteItem(model);

            var nullAction = ApplicationRobot.GetNextEstablishingAction(model, words);
            Assert.IsNull(nullAction);

            // Check there is now a single action to complete the text.
            var completionAction = ApplicationRobot.GetNextCompletionAction(model, words);
            Assert.IsNotNull(completionAction);
            Assert.IsTrue(completionAction.IsComplete);
            completionAction.ExecuteItem(model);

            return count;
        }

        private static int CountClicks(ApplicationModel model, string[] words)
        {
            var clicks = CountClicks(model, words, 0.0);
            return clicks;
        }

        private class EmptyEnvironment : DefaultWriterEnvironment, IWriterEnvironment
        {
            /// <summary>
            /// Dictionary of words, listed from most likely to least likely.
            /// </summary>
            /// <returns>List of words.</returns>
            public IEnumerable<string> GetOrderedSeedWords()
            {
                return new string[0];
            }
        }

        private static void MultiTest(string[] words, int expectedFirstClicks, int expectedSecondClicks, int expectedEmptyClicks, int expectedClicksWithRandomErrors)
        {
            var model = new ApplicationModel() { MaxNextSuggestionsCount = 9 };

            var actualFirstClicks = CountClicks(model, words);
            Assert.AreEqual(expectedFirstClicks, actualFirstClicks);

            var actualSecondClicks = CountClicks(model, words);
            Assert.AreEqual(expectedSecondClicks, actualSecondClicks);

            var emptyEnvironment = new EmptyEnvironment();
            var emptyModel = new ApplicationModel(emptyEnvironment);

            var actualEmptyClicks = CountClicks(emptyModel, words);
            Assert.AreEqual(expectedEmptyClicks, actualEmptyClicks);

            var errorModel = new ApplicationModel() { MaxNextSuggestionsCount = 9 };
            var actualClicksWithRandomErrors = CountClicks(errorModel, words, 0.2);
            Assert.AreEqual(expectedClicksWithRandomErrors, actualClicksWithRandomErrors);
        }

        private static void MultiTest(string sentence, int expectedFirstClicks, int expectedSecondClicks, int expectedEmptyClicks, int expectedClicksWithRandomErrors)
        {
            var words = sentence.Split(' ');
            MultiTest(words, expectedFirstClicks, expectedSecondClicks, expectedEmptyClicks, expectedClicksWithRandomErrors);
        }

        [Test]
        public void ThisIsTheDawningOfTheAgeOfAquariusTest()
        {
            MultiTest("this is the dawning of the age of aquarius", 33, 1, 104, 90);
        }

        [Test]
        public void TheQuickBrownFoxJumpsOverALazyDogTest()
        {
            MultiTest("the quick brown fox jumps over a lazy dog", 39, 1, 175, 96);
        }

        [Test]
        public void HelloWorldTest()
        {
            // TODO: Find out why entering HELLO WORLD predicts HELLO WORLD HELLO WORLD as the next sentence.
            MultiTest("hello world", 10, 1, 41, 10);
        }

        [Test]
        public void IzzyWizzyLetsGetBusyTest()
        {
            MultiTest("izzy wizzy lets get busy", 31, 1, 73, 133);
        }

        [Test]
        public void ShareAndEnjoyKoreanTest()
        {
            MultiTest("공유하 고 즐기십시오", 94, 1, 67, 512);
        }

        [Test]
        public void ShareAndEnjoyCantoneseTest()
        {
            MultiTest("分享 同 享受", 53, 1, 33, 176);
        }

        [Test]
        public void ShareAndEnjoyThaiTest()
        {
            MultiTest("แบ่งปัน และ เพลิดเพลิน", 139, 1, 99, 571);
        }

        [Test]
        public void BoldlyGoTest()
        {
            var model = new ApplicationModel();

            var script = new[]
            {
                "space",
                "the final frontier",
                "these are the voyages of the starship enterprise",
                "its five year mission",
                "to explore strange new worlds",
                "to seek out new life",
                "and new civilizations",
                "to boldly go where no man has gone before"
            };

            for (var line = 0; line < script.Length - 1; line++)
            {
                var words = script[line].Split(' ');

                ApplicationRobotAction action;
                do
                {
                    action = ApplicationRobot.GetNextCompletionAction(model, words);
                    action.ExecuteItem(model);
                }
                while (!action.IsComplete);
            }

            var lastWords = script[^1].Split(' ');
            for (var action = ApplicationRobot.GetNextEstablishingAction(model, lastWords);
                action != null;
                action = ApplicationRobot.GetNextEstablishingAction(model, lastWords))
            {
                action.ExecuteItem(model);
            }
            var lastAction = ApplicationRobot.GetNextCompletionAction(model, lastWords);
            Assert.AreEqual(ApplicationRobotActionTarget.Tail, lastAction.Target);
            Assert.AreEqual(0, lastAction.Index);
        }

        [Test]
        public void IzzyWizzyTest()
        {
            var model = new ApplicationModel(new EmptyEnvironment());

            var count = 0;
            ApplicationRobotAction action;
            do
            {
                action = ApplicationRobot.GetNextCompletionAction(model, "izzy", "wizzy", "lets", "get", "busy");

                var item = action.GetItem(model);
                Debug.WriteLine($"{count}: {action.Target} {action.Index}.{action.SubIndex} - {item.GetType().Name} - {item}");

                action.ExecuteItem(model);
                count++;

                Assert.IsTrue(count < 200);
            }
            while (!action.IsComplete);
        }

        private void Establish(ApplicationModel model, params string[] words)
        {
            var action = ApplicationRobot.GetNextEstablishingAction(model, words);
            while (action != null)
            {
                action.ExecuteItem(model);
                action = ApplicationRobot.GetNextEstablishingAction(model, words);
            }
        }

        [Test]
        public void HelloWordNotIsTest()
        {
            var model = new ApplicationModel();
            Establish(model, "hello", "world");

            Assert.IsInstanceOf<HeadWordItem>(model.HeadItems[1]);
            Assert.AreEqual("hello", model.HeadItems[1].Content);
            model.HeadItems[1].Execute(null);

            Assert.IsInstanceOf<GhostWordItem>(model.HeadItems[2]);
            Assert.AreEqual("world", model.HeadItems[2].Content);
            /* TODO: This test is revealing nonsensical results currently!
            Assert.IsInstanceOf<GhostStopItem>(model.HeadItems[3]);
            model.HeadItems[3].Execute(null);

            Assert.IsInstanceOf<HeadStartItem>(model.HeadItems[0]);
            Assert.IsInstanceOf<GhostWordItem>(model.HeadItems[1]);
            Assert.AreEqual("HELLO", model.HeadItems[1].Content);
            Assert.IsInstanceOf<GhostWordItem>(model.HeadItems[2]);
            Assert.AreEqual("WORLD", model.HeadItems[2].Content);
            Assert.IsInstanceOf<GhostStopItem>(model.HeadItems[3]);
            */
        }
    }
}
