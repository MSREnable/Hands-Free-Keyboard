using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Data;
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
        /// <param name="utterance">The utterance.</param>
        void IWriterEnvironment.SaveUtterance(UtteranceData utterance)
        {
            _ = SaveUtteranceAsync(utterance);
        }

        private async Task SaveUtteranceAsync(UtteranceData utterance)
        {
            await AttachHistoryFileAsync();

            var line = utterance.ToLine();
            await FileIO.AppendLinesAsync(_historyFile, new[] { line });
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
        IAsyncEnumerable<UtteranceData> IWriterEnvironment.RecallUtterances()
        {
            return new UtteranceEnumerable(this);
        }

        private class UtteranceEnumerable : IAsyncEnumerable<UtteranceData>
        {
            private readonly UwpWriterEnvironment _uwpWriterEnvironment;

            public UtteranceEnumerable(UwpWriterEnvironment uwpWriterEnvironment)
            {
                _uwpWriterEnvironment = uwpWriterEnvironment;
            }

            public IAsyncEnumerator<UtteranceData> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new UtteranceEnumerator(_uwpWriterEnvironment);
            }

            private class UtteranceEnumerator : IAsyncEnumerator<UtteranceData>
            {
                private readonly UwpWriterEnvironment _uwpWriterEnvironment;
                private StreamReader _reader;

                public UtteranceEnumerator(UwpWriterEnvironment uwpWriterEnvironment)
                {
                    _uwpWriterEnvironment = uwpWriterEnvironment;
                }

                public UtteranceData Current { get; private set; }

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

                    UtteranceData utterance;
                    var eof = false;
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
                            utterance = UtteranceData.FromLine(line);
                        }
                    }
                    while (!eof && utterance == null);

                    Current = utterance;

                    return !eof;
                }
            }
        }
    }
}
