using Microsoft.CognitiveServices.Speech;
using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Apps.Wpf
{
    internal class NarratorVocalizer : INarratorVocalizer
    {
        private readonly static string subscriptionKey = Environment.GetEnvironmentVariable("SpeechWriterAzureSubscriptionKey");
        private readonly static string region = Environment.GetEnvironmentVariable("SpeechWriterAzureRegion");
        private readonly static SpeechConfig config = SpeechConfig.FromSubscription(subscriptionKey, region);
        private readonly SpeechSynthesizer synthesizer = new SpeechSynthesizer(config);

        private readonly SemaphoreSlim _synthesizerReady = new SemaphoreSlim(1);

        // Speech synthesis to the default speaker.
        public async Task SynthesisToSpeakerAsync(string text)
        {
            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Debug.WriteLine($"Speech synthesized to speaker for text [{text}]");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    Debug.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Debug.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Debug.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Debug.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }

            _synthesizerReady.Release();
        }

        internal NarratorVocalizer()
        {
        }

        void INarratorVocalizer.DisplayWordPerMinuteEstimate(List<string> spokenWords, TimeSpan speechTime)
        {
        }

        async Task INarratorVocalizer.SpeakSsmlAsync(string ssml)
        {
            await _synthesizerReady.WaitAsync();
            _ = SynthesisToSpeakerAsync(ssml);
        }
    }
}
