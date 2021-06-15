using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Interface for surfacing vocalization support for Narrator.
    /// </summary>
    public interface INarratorVocalizer
    {
        /// <summary>
        /// Speak the given SSML text and then release the given semaphore.
        /// </summary>
        /// <param name="ssml"></param>
        Task SpeakSsmlAsync(string ssml);

        /// <summary>
        /// Display an estimate of how fast words were entered.
        /// </summary>
        /// <param name="spokenWords"></param>
        /// <param name="speechTime"></param>
        void DisplayWordPerMinuteEstimate(List<string> spokenWords, TimeSpan speechTime);
    }
}
