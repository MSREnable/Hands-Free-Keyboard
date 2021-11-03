using Microsoft.Research.SpeechWriter.Core.Automation;
using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.Core.Items;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    public class ApplicationRobotTest
    {
        private class TracingWriterEnvironment : DefaultWriterEnvironment, IWriterEnvironment
        {
            private DateTimeOffset _clock = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
            internal readonly List<string> _trace = new List<string>();

            /// <summary>
            /// Get the current time.
            /// </summary>
            /// <returns>The local time.</returns>
            DateTimeOffset IWriterEnvironment.GetTimestamp()
            {
                var timestamp = _clock;
                _clock += TimeSpan.FromSeconds(1);

                return timestamp;
            }

            /// <summary>
            /// Save a trace ine.
            /// </summary>
            /// <param name="trace"></param>
            /// <returns>The line to trace.</returns>
            Task IWriterEnvironment.SaveTraceAsync(string trace)
            {
                _trace.Add(trace);
                return Task.CompletedTask;
            }
        }

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
                    else if (suggestion is ExtendedSuggestedWordItem)
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

        private static void SaveTrace(ApplicationModel model, [CallerMemberName] string traceName = null)
        {
            Assert.IsInstanceOf<TracingWriterEnvironment>(model.Environment);

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var traceTarget = Path.Combine(desktop, "RobotTestTrace");
            Directory.CreateDirectory(traceTarget);

            var existingFiles = Directory.GetFiles(traceTarget, traceName + ".*.log");
            var maxIndex = 0;
            foreach (var existingFile in existingFiles)
            {
                Assert.IsTrue(existingFile.EndsWith(".log"));
                var strippedFile = existingFile.Substring(0, existingFile.Length - 4);
                var dot = strippedFile.LastIndexOf('.');
                Assert.AreNotEqual(-1, dot);
                var indexString = strippedFile.Substring(dot + 1);
                var index = int.Parse(indexString);
                if (maxIndex < index)
                {
                    maxIndex = index;
                }
            }

            var targetFile = Path.Combine(traceTarget, traceName + '.' + (maxIndex + 1).ToString() + ".log");
            using (var writer = File.CreateText(targetFile))
            {
                var environment = (TracingWriterEnvironment)model.Environment;

                foreach (var line in environment._trace)
                {
                    writer.WriteLine(line);
                }
            }
        }

        private static int CountClicks(ApplicationModel model, TileSequence words, double errorRate, string traceName)
        {
            Assert.IsInstanceOf<TracingWriterEnvironment>(model.Environment);

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
                if (item is InterstitialNonItem)
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

            SaveTrace(model, traceName);

            return count;
        }

        private static int CountClicks(ApplicationModel model, TileSequence sequence, string traceName)
        {
            var clicks = CountClicks(model, sequence, 0.0, traceName);
            return clicks;
        }

        private class EmptyEnvironment : TracingWriterEnvironment, IWriterEnvironment
        {
            /// <summary>
            /// Dictionary of words, listed from most likely to least likely.
            /// </summary>
            /// <returns>List of words.</returns>
            IEnumerable<string> IWriterEnvironment.GetOrderedSeedWords()
            {
                return new string[0];
            }

            IEnumerable<char> IWriterEnvironment.GetAdditionalSymbols()
            {
                return new char[0];
            }
        }

        private static void MultiTest(TileSequence words, int expectedFirstClicks, int expectedSecondClicks, int expectedEmptyClicks, int expectedClicksWithRandomErrors, [CallerMemberName] string caller = null)
        {
            var model = new ApplicationModel(new TracingWriterEnvironment()) { DisplayRows = 9 };

            var actualFirstClicks = CountClicks(model, words, caller + ".first");
            Assert.AreEqual(expectedFirstClicks, actualFirstClicks, "first pass");

            var actualSecondClicks = CountClicks(model, words, caller + ".second");
            Assert.AreEqual(expectedSecondClicks, actualSecondClicks, "second pass");

            var emptyEnvironment = new EmptyEnvironment();
            var emptyModel = new ApplicationModel(emptyEnvironment);

            var actualEmptyClicks = CountClicks(emptyModel, words, caller + ".empty");
            Assert.AreEqual(expectedEmptyClicks, actualEmptyClicks, "from empty");

            var errorModel = new ApplicationModel(new TracingWriterEnvironment()) { DisplayRows = 9 };
            var actualClicksWithRandomErrors = CountClicks(errorModel, words, 0.2, caller + ".erred");
            Assert.AreEqual(expectedClicksWithRandomErrors, actualClicksWithRandomErrors, "with errors");
        }

        private static void MultiTest(string sentence, int expectedFirstClicks, int expectedSecondClicks, int expectedEmptyClicks, int expectedClicksWithRandomErrors, [CallerMemberName] string caller = null)
        {
            var words = TileSequence.FromRaw(sentence);
            MultiTest(words, expectedFirstClicks, expectedSecondClicks, expectedEmptyClicks, expectedClicksWithRandomErrors, caller);
        }

        [Test]
        public void ThisIsTheDawningOfTheAgeOfAquariusTest()
        {
            MultiTest("this is the dawning of the age of aquarius", 32, 1, 117, 83);
        }

        [Test]
        public void TheQuickBrownFoxJumpsOverALazyDogTest()
        {
            MultiTest("the quick brown fox jumps over a lazy dog", 37, 1, 186, 97);
        }

        [Test]
        public void HelloWorldTest()
        {
            // TODO: Find out why entering HELLO WORLD predicts HELLO WORLD HELLO WORLD as the next sentence.
            MultiTest("hello world", 11, 1, 45, 11);
        }

        [Test]
        public void IzzyWizzyLetsGetBusyTest()
        {
            MultiTest("izzy wizzy lets get busy", 29, 1, 77, 104);
        }

        [Test]
        public void ShareAndEnjoyKoreanTest()
        {
            MultiTest("공유하 고 즐기십시오", 93, 1, 65, 521);
        }

        [Test]
        public void ShareAndEnjoyCantoneseTest()
        {
            MultiTest("分享 同 享受", 53, 1, 33, 175);
        }

        [Test]
        public void ShareAndEnjoyThaiTest()
        {
            MultiTest("แบ่งปัน และ เพลิดเพลิน", 140, 1, 94, 620);
        }

        [Test]
        public void PunctuationTest()
        {
            MultiTest("That'll be $10, please!", 34, 1, 104, 182);
        }

        [Test]
        public void BoldlyGoTest()
        {
            var model = new ApplicationModel(new TracingWriterEnvironment());

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
                var lineSequence = TileSequence.FromRaw(script[line]);

                ApplicationRobotAction action;
                do
                {
                    action = ApplicationRobot.GetNextCompletionAction(model, lineSequence);
                    action.ExecuteItem(model);
                }
                while (!action.IsComplete);
            }

            var sequence = TileSequence.FromRaw(script[^1]);
            for (var action = ApplicationRobot.GetNextEstablishingAction(model, sequence);
                action != null;
                action = ApplicationRobot.GetNextEstablishingAction(model, sequence))
            {
                action.ExecuteItem(model);
            }
            var lastAction = ApplicationRobot.GetNextCompletionAction(model, sequence);
            Assert.AreEqual(ApplicationRobotActionTarget.Tail, lastAction.Target);
            Assert.AreEqual(0, lastAction.Index);

            SaveTrace(model);
        }

        [Test]
        public void IzzyWizzyTest()
        {
            var model = new ApplicationModel(new EmptyEnvironment());

            var count = 0;
            ApplicationRobotAction action;
            do
            {
                var sequence = TileSequence.FromRaw("izzy wizzy lets get busy");
                action = ApplicationRobot.GetNextCompletionAction(model, sequence);

                var item = action.GetItem(model);
                Debug.WriteLine($"{count}: {action.Target} {action.Index}.{action.SubIndex} - {item.GetType().Name} - {item}");

                action.ExecuteItem(model);
                count++;

                Assert.IsTrue(count < 200);
            }
            while (!action.IsComplete);
        }

        private void Establish(ApplicationModel model, string words)
        {
            var sequence = TileSequence.FromRaw(words);
            var action = ApplicationRobot.GetNextEstablishingAction(model, sequence);
            while (action != null)
            {
                action.ExecuteItem(model);
                action = ApplicationRobot.GetNextEstablishingAction(model, sequence);
            }
        }

        [Test]
        public void HelloWordNotIsTest()
        {
            var model = new ApplicationModel(new TracingWriterEnvironment());
            Establish(model, "hello world");

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

            SaveTrace(model);
        }

        private class PersistantEnvironment : TracingWriterEnvironment, IWriterEnvironment
        {
            private readonly StringBuilder _builder = new StringBuilder(string.Empty);

            /// <summary>
            /// Persist an utterance.
            /// </summary>
            /// <param name="utterance">The utterance</param>
            Task IWriterEnvironment.SaveUtteranceAsync(string utterance)
            {
                _builder.AppendLine(utterance);
                return Task.CompletedTask;
            }

            /// <summary>
            /// Get utterance reader.
            /// </summary>
            /// <returns>The collection of utterances.</returns>
            Task<TextReader> IWriterEnvironment.RecallUtterancesAsync()
            {
                var text = _builder.ToString();
                var reader = new StringReader(text);
                return Task.FromResult<TextReader>(reader);
            }
        }

        private static async Task CheckRecallAsync(string text, int expectedFirst, int expectedSecond, [CallerMemberName] string caller = null)
        {
            var sequence = TileSequence.FromRaw(text);

            var environment = new PersistantEnvironment();
            var modelFirst = new ApplicationModel(environment);

            var actualFirst = CountClicks(modelFirst, sequence, caller + ".first");

            var modelSecond = new ApplicationModel(environment);
            await modelSecond.LoadUtterancesAsync();

            var actualSecond = CountClicks(modelSecond, sequence, caller + ".second");

            Assert.AreEqual(expectedFirst, actualFirst);
            Assert.AreEqual(expectedSecond, actualSecond);
        }

        [Test]
        public async Task ShareAndEnjoyPersistance()
        {
            await CheckRecallAsync("share and enjoy, share and enjoy, journey though life with a plastic boy, or girl by your side", 61, 3);
        }

        [Test]
        public void EmptyHelloWorldTest()
        {
            var sequence = TileSequence.FromRaw("hello world");
            var environment = new EmptyEnvironment();
            var model = new ApplicationModel(environment);

            var count = 0;
            bool done;
            do
            {
                var action = ApplicationRobot.GetNextCompletionAction(model, sequence);
                action.ExecuteItem(model);
                count++;

                done = action.IsComplete;
            }
            while (!done);

            SaveTrace(model);

            Assert.AreEqual(count + 1, environment._trace.Count);

            var expected = new[]
            {
                "<InterstitialSpellingItem TS=\"2000-01-01T00:00:01.0000000+00:00\" />",
                "<InterstitialUnicodeItem TS=\"2000-01-01T00:00:03.0000000+00:00\" />",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:05.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"3288\"><SuggestedUnicodeItem Prefix=\"\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"\" Code=\"3320\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:07.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"170\"><SuggestedUnicodeItem Prefix=\"\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"\" Code=\"202\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:09.0000000+00:00\" _lowerBound=\"62\" _upperLimit=\"82\"><SuggestedUnicodeItem Prefix=\"\" Code=\"95\" /><SuggestedUnicodeItem Prefix=\"\" Code=\"114\" /></InterstitialGapItem>",
                "<SuggestedUnicodeItem TS=\"2000-01-01T00:00:11.0000000+00:00\" Prefix=\"\" Code=\"104\" />",
                "<InterstitialUnicodeItem TS=\"2000-01-01T00:00:13.0000000+00:00\" />",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:15.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"3288\"><SuggestedUnicodeItem Prefix=\"h\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"h\" Code=\"3320\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:17.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"170\"><SuggestedUnicodeItem Prefix=\"h\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"h\" Code=\"202\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:19.0000000+00:00\" _lowerBound=\"62\" _upperLimit=\"82\"><SuggestedUnicodeItem Prefix=\"h\" Code=\"95\" /><SuggestedUnicodeItem Prefix=\"h\" Code=\"114\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:21.0000000+00:00\" _lowerBound=\"65\" _upperLimit=\"73\"><SuggestedUnicodeItem Prefix=\"h\" Code=\"98\" /><SuggestedUnicodeItem Prefix=\"h\" Code=\"105\" /></InterstitialGapItem>",
                "<SuggestedUnicodeItem TS=\"2000-01-01T00:00:23.0000000+00:00\" Prefix=\"h\" Code=\"101\" />",
                "<InterstitialUnicodeItem TS=\"2000-01-01T00:00:25.0000000+00:00\" />",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:27.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"3288\"><SuggestedUnicodeItem Prefix=\"he\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"he\" Code=\"3320\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:29.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"170\"><SuggestedUnicodeItem Prefix=\"he\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"he\" Code=\"202\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:31.0000000+00:00\" _lowerBound=\"62\" _upperLimit=\"82\"><SuggestedUnicodeItem Prefix=\"he\" Code=\"95\" /><SuggestedUnicodeItem Prefix=\"he\" Code=\"114\" /></InterstitialGapItem>",
                "<SuggestedUnicodeItem TS=\"2000-01-01T00:00:33.0000000+00:00\" Prefix=\"he\" Code=\"108\" />",
                "<SuggestedSpellingItem TS=\"2000-01-01T00:00:35.0000000+00:00\" Prefix=\"hel\" Symbol=\"l\" />",
                "<InterstitialUnicodeItem TS=\"2000-01-01T00:00:37.0000000+00:00\" />",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:39.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"3288\"><SuggestedUnicodeItem Prefix=\"hell\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"hell\" Code=\"3320\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:41.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"170\"><SuggestedUnicodeItem Prefix=\"hell\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"hell\" Code=\"202\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:43.0000000+00:00\" _lowerBound=\"62\" _upperLimit=\"82\"><SuggestedUnicodeItem Prefix=\"hell\" Code=\"95\" /><SuggestedUnicodeItem Prefix=\"hell\" Code=\"114\" /></InterstitialGapItem>",
                "<SuggestedUnicodeItem TS=\"2000-01-01T00:00:45.0000000+00:00\" Prefix=\"hell\" Code=\"111\" />",
                "<SuggestedSpellingWordItem TS=\"2000-01-01T00:00:47.0000000+00:00\" Content=\"hello\" />",
                "<InterstitialSpellingItem TS=\"2000-01-01T00:00:49.0000000+00:00\" />",
                "<InterstitialUnicodeItem TS=\"2000-01-01T00:00:51.0000000+00:00\" />",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:53.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"3288\"><SuggestedUnicodeItem Prefix=\"\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"\" Code=\"3320\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:55.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"170\"><SuggestedUnicodeItem Prefix=\"\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"\" Code=\"202\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:00:57.0000000+00:00\" _lowerBound=\"83\" _upperLimit=\"114\"><SuggestedUnicodeItem Prefix=\"\" Code=\"116\" /><SuggestedUnicodeItem Prefix=\"\" Code=\"146\" /></InterstitialGapItem>",
                "<SuggestedUnicodeItem TS=\"2000-01-01T00:00:59.0000000+00:00\" Prefix=\"\" Code=\"119\" />",
                "<SuggestedSpellingItem TS=\"2000-01-01T00:01:01.0000000+00:00\" Prefix=\"w\" Symbol=\"o\" />",
                "<InterstitialUnicodeItem TS=\"2000-01-01T00:01:03.0000000+00:00\" />",
                "<InterstitialGapItem TS=\"2000-01-01T00:01:05.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"3288\"><SuggestedUnicodeItem Prefix=\"wo\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"wo\" Code=\"3320\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:01:07.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"170\"><SuggestedUnicodeItem Prefix=\"wo\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"wo\" Code=\"202\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:01:09.0000000+00:00\" _lowerBound=\"62\" _upperLimit=\"82\"><SuggestedUnicodeItem Prefix=\"wo\" Code=\"95\" /><SuggestedUnicodeItem Prefix=\"wo\" Code=\"114\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:01:11.0000000+00:00\" _lowerBound=\"77\" _upperLimit=\"85\"><SuggestedUnicodeItem Prefix=\"wo\" Code=\"110\" /><SuggestedUnicodeItem Prefix=\"wo\" Code=\"117\" /></InterstitialGapItem>",
                "<SuggestedUnicodeItem TS=\"2000-01-01T00:01:13.0000000+00:00\" Prefix=\"wo\" Code=\"114\" />",
                "<SuggestedSpellingItem TS=\"2000-01-01T00:01:15.0000000+00:00\" Prefix=\"wor\" Symbol=\"l\" />",
                "<InterstitialUnicodeItem TS=\"2000-01-01T00:01:17.0000000+00:00\" />",
                "<InterstitialGapItem TS=\"2000-01-01T00:01:19.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"3288\"><SuggestedUnicodeItem Prefix=\"worl\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"worl\" Code=\"3320\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:01:21.0000000+00:00\" _lowerBound=\"0\" _upperLimit=\"170\"><SuggestedUnicodeItem Prefix=\"worl\" Code=\"33\" /><SuggestedUnicodeItem Prefix=\"worl\" Code=\"202\" /></InterstitialGapItem>",
                "<InterstitialGapItem TS=\"2000-01-01T00:01:23.0000000+00:00\" _lowerBound=\"62\" _upperLimit=\"82\"><SuggestedUnicodeItem Prefix=\"worl\" Code=\"95\" /><SuggestedUnicodeItem Prefix=\"worl\" Code=\"114\" /></InterstitialGapItem>",
                "<SuggestedUnicodeItem TS=\"2000-01-01T00:01:25.0000000+00:00\" Prefix=\"worl\" Code=\"100\" />",
                "<SuggestedSpellingWordItem TS=\"2000-01-01T00:01:27.0000000+00:00\" Content=\"world\" />",
                "<TailStopItem TS=\"2000-01-01T00:01:29.0000000+00:00\" />",
                "<U Started=\"2000-01-01T00:00:00.0000000+00:00\" Duration=\"88000\" KeyCount=\"45\">hello world</U>"
            };

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], environment._trace[i]);
            }
            Assert.AreEqual(expected.Length, count + 1);

        }
    }
}
