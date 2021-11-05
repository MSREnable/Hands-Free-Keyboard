using Microsoft.Research.SpeechWriter.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Research.SpeechWriter.Apps.Wpf
{
    internal class WpfEnvironment : DefaultWriterEnvironment, IWriterEnvironment
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        internal static readonly string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SpeechWriter");
        private static readonly string UtterancesPath = Path.Combine(DataPath, "utterances.log");
        private static readonly string TracePath = Path.Combine(DataPath, "trace.log");

        Task<TextReader> IWriterEnvironment.RecallUtterancesAsync()
        {
            Stream stream;

            var parent = Path.GetDirectoryName(UtterancesPath);
            Directory.CreateDirectory(parent);

            try
            {
                stream = File.OpenRead(UtterancesPath);
            }
            catch (FileNotFoundException)
            {
                stream = Stream.Null;
            }

            var reader = new StreamReader(stream);

            return Task.FromResult<TextReader>(reader);
        }

        async Task IWriterEnvironment.SaveTraceAsync(string trace)
        {
            await File.AppendAllLinesAsync(TracePath, new[] { trace });
        }

        async Task IWriterEnvironment.SaveUtteranceAsync(string utterance)
        {
            await File.AppendAllLinesAsync(UtterancesPath, new[] { utterance });
        }

        Task<bool> IWriterEnvironment.ShowSettingsAsync(WriterSettings settings)
        {
            MessageBox.Show("Settings go here");
            return Task.FromResult(false);
        }
    }
}
