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
        /// Get the user settings.
        /// </summary>
        WriterSettings Settings { get; }

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
        /// Try to capitalize a string as if it were the first word of a sentence.
        /// </summary>
        /// <param name="word">The text to be capitalized.</param>
        /// <returns>The word in the form it would appear if it were the first word
        /// of a sentence. If it is not a word, but punctuation, etc., return null.</returns>
        /// <remarks>In a typical implementation a quotation mark will return null, 
        /// 'an' in lowercase will return 'An' with the first letter capitalized,
        /// a property noun like 'I' will return the word unchanged (not null),
        /// a number (like 42) will return the number unchanged.</remarks>
        string TryCapitalizeFirstWord(string word);

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

        /// <summary>
        /// Show settings to the user.
        /// </summary>
        /// <param name="settings">The settings object to be updated.</param>
        /// <returns>True if settings updates.</returns>
        Task<bool> ShowSettingsAsync(WriterSettings settings);
    }
}
