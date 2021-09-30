using Microsoft.CognitiveServices.Speech;
using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Research.SpeechWriter.Apps.Uwp
{
    internal class NarratorVocalization : INarratorVocalizer
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

        private void Initialize(string language)
        {
        }

        internal static NarratorVocalization Create(MediaElement mediaElement, string language)
        {
            var vocalization = new NarratorVocalization();
            return vocalization;
        }

        async Task INarratorVocalizer.SpeakSsmlAsync(string ssml)
        {
            await _synthesizerReady.WaitAsync();
            _ = SynthesisToSpeakerAsync(ssml);
        }

        void INarratorVocalizer.DisplayWordPerMinuteEstimate(List<string> spokenWords, TimeSpan speechTime)
        {
            var join = string.Join(' ', spokenWords);
            var trueWordsPerMinute = spokenWords.Count / speechTime.TotalMinutes;
            var standardWordPerMinute = (join.Length / 5.0) / speechTime.TotalMinutes;
            var title = $"True wpm = {trueWordsPerMinute:0.0}, standard wpm = {standardWordPerMinute:0.0}";
            ApplicationView.GetForCurrentView().Title = title;
        }

    }
}
