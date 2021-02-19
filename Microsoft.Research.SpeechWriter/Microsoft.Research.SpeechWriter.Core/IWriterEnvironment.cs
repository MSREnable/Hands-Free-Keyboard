using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Environmental objects for writer.
    /// </summary>
    public interface IWriterEnvironment : IComparer<string>
    {
        /// <summary>
        /// Dictionary of words, listed from most likely to least likely.
        /// </summary>
        /// <returns>List of words.</returns>
        IEnumerable<string> GetOrderedSeedWords();

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="words">The words of the utterance.</param>
        void SaveUtterance(string[] words);

        /// <summary>
        /// Get utterance reader.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IAsyncEnumerable<string[]> RecallUtterances();
    }
}
