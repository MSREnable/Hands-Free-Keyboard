using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Research.SpeechWriter.Apps.Uwp
{
    internal class NarratorVocalization : INarratorVocalizer
    {
        private readonly MediaElement _mediaElement;

        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        private readonly SemaphoreSlim _mediaReady = new SemaphoreSlim(1);

        private NarratorVocalization(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.MediaEnded += (s, e) => _mediaReady.Release();

        }

        private void Initialize(string language)
        {
            var voiceChoice = new List<VoiceInformation>();
            foreach (var voice in SpeechSynthesizer.AllVoices)
            {
                if (voice.Language.StartsWith(language))
                {
                    voiceChoice.Add(voice);
                }
            }

            if (voiceChoice.Count != 0)
            {
                _synthesizer.Voice = voiceChoice[0];
            }
        }

        internal static NarratorVocalization Create(MediaElement mediaElement, string language)
        {
            var vocalization = new NarratorVocalization(mediaElement);
            vocalization.Initialize(language);
            return vocalization;
        }

        async Task INarratorVocalizer.SpeakSsmlAsync(string ssml)
        {
            Debug.WriteLine("Waiting for media");
            await _mediaReady.WaitAsync();
            Debug.WriteLine("Media ready");

            var stream = await _synthesizer.SynthesizeTextToStreamAsync(ssml);
            _mediaElement.SetSource(stream, stream.ContentType);
            _mediaElement.Play();
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
