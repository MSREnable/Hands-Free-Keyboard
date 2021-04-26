using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
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
        /// <param name="words">The words of the utterance.</param>
        void IWriterEnvironment.SaveUtterance(string[] words)
        {
            _ = SaveUtteranceAsync(words);
        }

        private async Task SaveUtteranceAsync(string[] words)
        {
            await AttachHistoryFileAsync();

            var utterance = string.Join(' ', words);
            await FileIO.AppendLinesAsync(_historyFile, new[] { utterance });
        }

        private async Task AttachHistoryFileAsync()
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
        }

        /// <summary>
        /// Recall persisted utterances.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IAsyncEnumerable<string[]> IWriterEnvironment.RecallUtterances()
        {
            return new UtteranceEnumerable(this);
        }

        private class UtteranceEnumerable : IAsyncEnumerable<string[]>
        {
            private readonly UwpWriterEnvironment _uwpWriterEnvironment;

            public UtteranceEnumerable(UwpWriterEnvironment uwpWriterEnvironment)
            {
                _uwpWriterEnvironment = uwpWriterEnvironment;
            }

            public IAsyncEnumerator<string[]> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new UtteranceEnumerator(_uwpWriterEnvironment);
            }

            private class UtteranceEnumerator : IAsyncEnumerator<string[]>
            {
                private readonly UwpWriterEnvironment _uwpWriterEnvironment;
                private StreamReader _reader;

                public UtteranceEnumerator(UwpWriterEnvironment uwpWriterEnvironment)
                {
                    _uwpWriterEnvironment = uwpWriterEnvironment;
                }

                public string[] Current { get; private set; }

                public ValueTask DisposeAsync()
                {
                    if (_reader != null)
                    {
                        _reader.Close();
                    }

                    return new ValueTask(Task.CompletedTask);
                }

                public async ValueTask<bool> MoveNextAsync()
                {
                    await _uwpWriterEnvironment.AttachHistoryFileAsync();

                    if (_reader == null)
                    {
                        var stream = await _uwpWriterEnvironment._historyFile.OpenSequentialReadAsync();
                        _reader = new StreamReader(stream.AsStreamForRead());
                    }

                    string[] utterance;
                    var eof = false; ;
                    do
                    {
                        var line = await _reader.ReadLineAsync();

                        if (line == null)
                        {
                            utterance = null;
                            eof = true;
                        }
                        else if (string.IsNullOrWhiteSpace(line))
                        {
                            utterance = null;
                        }
                        else
                        {
                            utterance = line.Split(' ');
                        }
                    }
                    while (!eof && utterance == null); ;

                    Current = utterance;

                    return !eof;
                }
            }
        }
    }
}
