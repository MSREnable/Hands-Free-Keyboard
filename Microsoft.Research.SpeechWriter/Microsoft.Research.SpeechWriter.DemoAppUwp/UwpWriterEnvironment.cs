using Microsoft.Research.SpeechWriter.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    internal class UwpWriterEnvironment : DefaultWriterEnvironment, IWriterEnvironment
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private IStorageFile _historyFile;

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="utterance">The utterance.</param>
        async Task IWriterEnvironment.SaveUtteranceAsync(string utterance)
        {
            var file = await GetHistoryFileAsync();
            await FileIO.AppendLinesAsync(file, new[] { utterance });
        }

        internal async Task<IStorageFile> GetHistoryFileAsync()
        {
            if (_historyFile == null)
            {
                await _semaphore.WaitAsync();
                if (_historyFile == null)
                {
                    var roamingFolder = ApplicationData.Current.RoamingFolder;
                    _historyFile = await roamingFolder.CreateFileAsync("Utterances.log", CreationCollisionOption.OpenIfExists);
                }
                _semaphore.Release();
            }
            return _historyFile;
        }

        /// <summary>
        /// Recall persisted utterances.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        async Task<TextReader> IWriterEnvironment.RecallUtterancesAsync()
        {
            var file = await GetHistoryFileAsync();
            var stream = await file.OpenSequentialReadAsync();
            var reader = new StreamReader(stream.AsStreamForRead());
            return reader;
        }
    }
}
