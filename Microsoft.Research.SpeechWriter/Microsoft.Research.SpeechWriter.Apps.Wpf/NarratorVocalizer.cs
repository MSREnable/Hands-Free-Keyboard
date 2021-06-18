using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Apps.Wpf
{
    internal class NarratorVocalizer : INarratorVocalizer
    {
        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        private readonly SemaphoreSlim _synthesizerReady = new SemaphoreSlim(1);

        internal NarratorVocalizer()
        {
            _synthesizer.SpeakCompleted += (s, e) => _synthesizerReady.Release();
        }

        void INarratorVocalizer.DisplayWordPerMinuteEstimate(List<string> spokenWords, TimeSpan speechTime)
        {
        }

        async Task INarratorVocalizer.SpeakSsmlAsync(string ssml)
        {
            await _synthesizerReady.WaitAsync();
            _ = Task.Run(() => _synthesizer.SpeakAsync(ssml));
        }
    }
}
