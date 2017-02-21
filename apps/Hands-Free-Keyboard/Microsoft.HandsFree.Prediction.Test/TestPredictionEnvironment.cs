using Microsoft.HandsFree.Prediction.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Prediction.Test
{
    public class TestPredictionEnvironment : IPredictionEnvironment
    {
        MemoryStream dynamicDictionaryStream;

        MemoryStream staticDictionaryStream;

        public TestPredictionEnvironment(string history)
        {
            Assert.IsNotNull(history);

            History = history;
        }

        public TestPredictionEnvironment() : this(string.Empty)
        {
        }

        public string History { get; set; }

        public int MaximumWordSuggestionCount
        {
            get; protected set;
        } = 7;

        public PredictionSettings PredictionSettings
        {
            get; protected set;
        } = new PredictionSettings();

        public BinaryWriter CreateDynamicDictionaryCache()
        {
            dynamicDictionaryStream = new MemoryStream();
            var writer = new BinaryWriter(dynamicDictionaryStream);
            return writer;
        }

        public BinaryWriter CreateStaticDictionaryCache()
        {
            staticDictionaryStream = new MemoryStream();
            var writer = new BinaryWriter(staticDictionaryStream);
            return writer;
        }

        public string GetHistoryText()
        {
            return History;
        }

        public BinaryReader OpenDynamicDictionaryCache()
        {
            var stream = dynamicDictionaryStream == null ? null : new MemoryStream(dynamicDictionaryStream.ToArray());
            var reader = stream == null ? null : new BinaryReader(stream);
            return reader;
        }

        public BinaryReader OpenStaticDictionaryCache()
        {
            var stream = staticDictionaryStream == null ? null : new MemoryStream(staticDictionaryStream.ToArray());
            var reader = stream == null ? null : new BinaryReader(stream);
            return reader;
        }

        public virtual void QueueWorkItem(Action action)
        {
            action();
        }

        public void RecordAcceptedSuggestion(int index, int seed, string suggestion)
        {
        }

        public void RecordHistory(string text, bool isInPrivate)
        {
            History += text + Environment.NewLine;
        }
    }
}
