using System.Collections.Generic;

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
        /// List of sentences, comprising a sequence of words.
        /// </summary>
        /// <returns>List of list of words.</returns>
        IEnumerable<IEnumerable<string>> GetSeedSentences();

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="words">The words of the utterance.</param>
        void SaveUtterance(string[] words);

        /// <summary>
        /// Recall persisted utterances.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IEnumerable<string[]> RecallUtterances();
    }
}
