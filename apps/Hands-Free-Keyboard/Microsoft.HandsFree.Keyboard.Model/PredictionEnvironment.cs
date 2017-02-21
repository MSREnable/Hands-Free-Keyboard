using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Engine;
using Microsoft.HandsFree.Settings;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Keyboard.Model
{
    class PredictionEnvironment : IPredictionEnvironment
    {
        static readonly string HistoryFilePath = SettingsDirectory.GetDefaultSettingsFilePath("spoken.txt");
        static readonly string AcceptedSuggestionFilePath = SettingsDirectory.GetDefaultSettingsFilePath("accepted.txt");
        static readonly string StaticDictionaryCacheFilePath = SettingsDirectory.GetDefaultSettingsFilePath("staticdictionary.dat");
        static readonly string DynamicDictionaryCacheFilePath = SettingsDirectory.GetDefaultSettingsFilePath("dynamicdictionary.dat");

        internal static readonly PredictionEnvironment Instance = new PredictionEnvironment();

        /// <summary>
        /// Private constructor.
        /// </summary>
        PredictionEnvironment()
        {
        }

        public PredictionSettings PredictionSettings
        {
            get { return AppSettings.Instance.Prediction; }
        }

        public int MaximumWordSuggestionCount
        {
            get { return 7; }
        }

        public void QueueWorkItem(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => action());
        }

        public string GetHistoryText()
        {
            var records = XmlFragmentHelper.ReadLog<Spoken>(HistoryFilePath);

            var text = string.Join(Environment.NewLine, from r in records select r.Text);

            return text;
        }

        public void RecordHistory(string text, bool isInPrivate)
        {
            if (!isInPrivate)
            {
                var record = new Spoken { UtcNowTicks = DateTime.UtcNow.Ticks, TickCount = Environment.TickCount, IsInPrivate = isInPrivate, Text = text };
                XmlFragmentHelper.WriteLog(HistoryFilePath, record);
            }
        }

        public void RecordAcceptedSuggestion(int index, int seed, string suggestion)
        {
            var record = new SuggestionAcceptance { UtcNow = DateTime.UtcNow.Ticks, TickCount = Environment.TickCount, Seed = seed, Index = index, Word = suggestion };
            XmlFragmentHelper.WriteLog(AcceptedSuggestionFilePath, record);
        }

        public BinaryWriter CreateStaticDictionaryCache()
        {
            var stream = File.Create(StaticDictionaryCacheFilePath);
            var writer = new BinaryWriter(stream);
            return writer;
        }

        public BinaryReader OpenStaticDictionaryCache()
        {
            BinaryReader reader;

            try
            {
                var stream = File.OpenRead(StaticDictionaryCacheFilePath);
                reader = new BinaryReader(stream);
            }
            catch (FileNotFoundException)
            {
                reader = null;
            }

            return reader;
        }

        static byte[] GetHistoryFileSignature()
        {
            var historyWriteTime = File.GetLastWriteTimeUtc(HistoryFilePath);
            var tickBytes = BitConverter.GetBytes(historyWriteTime.Ticks);

            return tickBytes;
        }

        public BinaryWriter CreateDynamicDictionaryCache()
        {
            var stream = File.Create(DynamicDictionaryCacheFilePath);
            var writer = new BinaryWriter(stream);

            var headerBytes = GetHistoryFileSignature();
            writer.Write(headerBytes);

            return writer;
        }

        public BinaryReader OpenDynamicDictionaryCache()
        {
            BinaryReader reader;

            try
            {
                var stream = File.OpenRead(DynamicDictionaryCacheFilePath);
                reader = new BinaryReader(stream);

                var expectedHeaderBytes = GetHistoryFileSignature();

                var actualHeaderBytes = reader.ReadBytes(expectedHeaderBytes.Length);

                var lim = 0;
                if (actualHeaderBytes.Length == expectedHeaderBytes.Length)
                {
                    while (lim < expectedHeaderBytes.Length && expectedHeaderBytes[lim] == actualHeaderBytes[lim])
                    {
                        lim++;
                    }
                }

                if (lim != actualHeaderBytes.Length)
                {
                    // Not all the header matches.
                    reader.Dispose();
                    reader = null;
                }
            }
            catch (FileNotFoundException)
            {
                reader = null;
            }

            return reader;
        }
    }
}
