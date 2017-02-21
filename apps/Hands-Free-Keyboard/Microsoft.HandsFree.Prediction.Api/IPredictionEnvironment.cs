using System;
using System.IO;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Prediction.Api
{
    /// <summary>
    /// Interface providing environment for performing predictions.
    /// </summary>
    public interface IPredictionEnvironment
    {
        /// <summary>
        /// The general settings object.
        /// </summary>
        PredictionSettings PredictionSettings { get; }

        /// <summary>
        /// Maximum number of suggestions the client is likely to consume.
        /// </summary>
        int MaximumWordSuggestionCount { get; }

        /// <summary>
        /// Queue a work item to be run later, potentially on a separate thread.
        /// </summary>
        /// <param name="action"></param>
        void QueueWorkItem(Action action);

        /// <summary>
        /// Get the text of all the historic events.
        /// </summary>
        /// <returns>All the recorded historic texts.</returns>
        string GetHistoryText();

        /// <summary>
        /// Record a new piece of history text.
        /// </summary>
        /// <param name="text">The historic text.</param>
        /// <param name="isInPrivate">Is the text uttered in private.</param>
        void RecordHistory(string text, bool isInPrivate);

        /// <summary>
        /// Record use of suggestion.
        /// </summary>
        /// <param name="index">User interface position of suggestion.</param>
        /// <param name="seed">Typically number of letters used to suggest a word.</param>
        /// <param name="suggestion">The suggested text.</param>
        void RecordAcceptedSuggestion(int index, int seed, string suggestion);

        /// <summary>
        /// Create or overwrite static dictionary cache.
        /// </summary>
        /// <returns></returns>
        BinaryWriter CreateStaticDictionaryCache();

        /// <summary>
        /// Open static dictionary cache, or return null if none found.
        /// </summary>
        /// <returns></returns>
        BinaryReader OpenStaticDictionaryCache();

        /// <summary>
        /// Create or overwrite dynamic dictionary cache.
        /// </summary>
        /// <returns></returns>
        BinaryWriter CreateDynamicDictionaryCache();

        /// <summary>
        /// Open dynamic dictionary cache, or return null if none found.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Will return null if no cache found or it was written after before last RecordHistory call.</remarks>
        BinaryReader OpenDynamicDictionaryCache();
    }
}
