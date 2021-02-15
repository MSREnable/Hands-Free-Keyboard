using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    class UwpWriterEnvironment : DefaultWriterEnvironment, IWriterEnvironment
    {
        private readonly IStorageFile _historyFile;

        internal UwpWriterEnvironment()
        {
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            var task = roamingFolder.CreateFileAsync("Utterances.log", CreationCollisionOption.OpenIfExists);
            _historyFile = task.GetResults();
        }

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="words">The words of the utterance.</param>
        void IWriterEnvironment.SaveUtterance(string[] words)
        {
            var utterance = string.Join(' ', words);
            _ = FileIO.AppendLinesAsync(_historyFile, new[] { utterance });
        }

        /// <summary>
        /// Recall persisted utterances.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IEnumerable<string[]> IWriterEnvironment.RecallUtterances()
        {
            var task = FileIO.ReadLinesAsync(_historyFile);
            var utterances = task.GetResults();
            foreach (var utterance in utterances)
            {
                var words = utterance.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                yield return words;
            }
        }
    }
}
