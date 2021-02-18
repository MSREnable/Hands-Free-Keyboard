using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    class UwpWriterEnvironment : DefaultWriterEnvironment, IWriterEnvironment
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private IStorageFile _historyFile;

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="words">The words of the utterance.</param>
        void IWriterEnvironment.SaveUtterance(string[] words)
        {
            _ = SaveUtteranceAsync(words);
        }

        private async Task SaveUtteranceAsync(string[] words)
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

            var utterance = string.Join(' ', words);
            await FileIO.AppendLinesAsync(_historyFile, new[] { utterance });
        }

        /// <summary>
        /// Recall persisted utterances.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IEnumerable<string[]> IWriterEnvironment.RecallUtterances()
        {
            //var task = FileIO.ReadLinesAsync(_historyFile);
            //var utterances = task.GetResults();
            var utterances = new string[0];
            foreach (var utterance in utterances)
            {
                var words = utterance.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                yield return words;
            }
        }
    }
}
