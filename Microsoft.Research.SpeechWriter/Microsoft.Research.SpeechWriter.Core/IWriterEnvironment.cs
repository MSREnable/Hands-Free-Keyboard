using Microsoft.Research.SpeechWriter.Core.Data;
using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Environmental objects for writer.
    /// </summary>
    public interface IWriterEnvironment : IComparer<string>
    {
        /// <summary>
        /// The language.
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Dictionary of words, listed from most likely to least likely.
        /// </summary>
        /// <returns>List of words.</returns>
        IEnumerable<string> GetOrderedSeedWords();

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="tiles">The words of the utterance.</param>
        /// <param name="start">The time the click that started the utterance was started.</param>
        /// <param name="duration">The time difference between the first and last click.</param>
        /// <param name="clickCount">The number of clicks used to form the utterance.</param>
        void SaveUtterance(TileSequence tiles,
            DateTimeOffset start,
            TimeSpan duration,
            int clickCount);

        /// <summary>
        /// Get utterance reader.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IAsyncEnumerable<TileSequence> RecallUtterances();
    }
}
