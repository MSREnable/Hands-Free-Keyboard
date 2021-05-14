using Microsoft.Research.SpeechWriter.Core.Data;
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
        void SaveUtterance(IReadOnlyList<TileData> tiles);

        /// <summary>
        /// Get utterance reader.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IAsyncEnumerable<IReadOnlyList<TileData>> RecallUtterances();
    }
}
