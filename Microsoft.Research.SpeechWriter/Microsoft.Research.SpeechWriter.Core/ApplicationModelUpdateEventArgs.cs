using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Model update details.
    /// </summary>
    public class ApplicationModelUpdateEventArgs : EventArgs
    {
        internal ApplicationModelUpdateEventArgs(IEnumerable<string> words,
            int prevoiusWordsLength,
            bool isComplete)
        {
            Words = new List<string>(words);
            PreviousWordsLength = prevoiusWordsLength;
            IsComplete = isComplete;
        }

        /// <summary>
        /// The list of head words.
        /// </summary>
        public IReadOnlyList<string> Words { get; }

        /// <summary>
        /// The number of words that was prevoiusly in the Words collection.
        /// </summary>
        /// <remarks>If this is less than the length of Words, words have been added.
        /// If it is greater words have been deleted. If it is the same, nothing has
        /// been added or removed.</remarks>
        public int PreviousWordsLength { get; }

        /// <summary>
        /// This is the final notification about the current utterance.
        /// </summary>
        public bool IsComplete { get; }
    }
}