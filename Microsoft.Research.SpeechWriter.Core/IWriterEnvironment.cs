using System.Collections.Generic;

namespace Microsoft.Research.RankWriter.Library
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
    }
}
