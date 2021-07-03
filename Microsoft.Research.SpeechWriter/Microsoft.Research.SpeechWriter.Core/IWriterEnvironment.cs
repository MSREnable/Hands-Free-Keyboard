using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
        /// Get additional seed symbols not included in any words.
        /// </summary>
        /// <returns></returns>
        IEnumerable<char> GetAdditionalSymbols();

        /// <summary>
        /// Get the current time.
        /// </summary>
        /// <returns>The local time.</returns>
        DateTimeOffset GetTimestamp();

        /// <summary>
        /// Save a trace ine.
        /// </summary>
        /// <param name="trace"></param>
        /// <returns>The line to trace.</returns>
        Task SaveTraceAsync(string trace);

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="utterance">The utterance</param>
        Task SaveUtteranceAsync(string utterance);

        /// <summary>
        /// Get utterance reader.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        Task<TextReader> RecallUtterancesAsync();
    }
}
