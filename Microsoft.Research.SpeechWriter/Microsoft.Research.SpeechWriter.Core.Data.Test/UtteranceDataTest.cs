using NUnit.Framework;
using System;

namespace Microsoft.Research.SpeechWriter.Core.Data.Test
{
    [Parallelizable(ParallelScope.All)]
    public class UtteranceDataTest
    {
        [Test]
        public void JackAmpJillTest()
        {
            var sequence = TileSequence.FromRaw("Jack & Jill");
            var started = new DateTimeOffset(1963, 12, 5, 12, 30, 45, 123, TimeSpan.FromMinutes(7 * 60));
            var duration = TimeSpan.FromSeconds(5 * 60);
            var keyCount = 42;

            var utterance = new UtteranceData(sequence, started, duration, keyCount);

            var line = utterance.ToLine();

            Assert.AreEqual("<U Started=\"1963-12-05T12:30:45.1230000+07:00\" Duration=\"300000\" KeyCount=\"42\"><T>Jack</T><T>&amp;</T><T>Jill</T></U>", line);

            var actual = UtteranceData.FromLine(line);

            Assert.AreEqual(sequence, actual.Sequence);
            Assert.AreEqual(started, actual.Started);
            Assert.AreEqual(duration, actual.Duration);
            Assert.AreEqual(keyCount, actual.KeyCount);
        }

        [Test]
        public void JackAndJillTest()
        {
            var sequence = TileSequence.FromRaw("Jack and Jill");
            var started = new DateTimeOffset(1963, 12, 5, 12, 30, 45, 123, TimeSpan.FromMinutes(7 * 60));
            var duration = TimeSpan.FromSeconds(5 * 60);
            var keyCount = 42;

            var utterance = new UtteranceData(sequence, started, duration, keyCount);

            var line = utterance.ToLine();

            Assert.AreEqual("<U Started=\"1963-12-05T12:30:45.1230000+07:00\" Duration=\"300000\" KeyCount=\"42\">Jack and Jill</U>", line);

            var actual = UtteranceData.FromLine(line);

            Assert.AreEqual(sequence, actual.Sequence);
            Assert.AreEqual(started, actual.Started);
            Assert.AreEqual(duration, actual.Duration);
            Assert.AreEqual(keyCount, actual.KeyCount);
        }
    }
}
