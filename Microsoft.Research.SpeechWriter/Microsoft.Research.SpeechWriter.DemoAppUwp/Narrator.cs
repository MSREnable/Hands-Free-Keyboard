using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    class Narrator
    {
        private readonly ApplicationModel _model;

        private readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        private readonly Queue<ApplicationModelUpdateEventArgs> _speech = new Queue<ApplicationModelUpdateEventArgs>();

        private readonly MediaElement TheMediaElement;

        private readonly SemaphoreSlim _mediaReady = new SemaphoreSlim(1);

        private int sent;

        private readonly string _language;

        private DateTimeOffset _speechStarted;

        private Narrator(ApplicationModel model, MediaElement mediaElement, string language)
        {
            _model = model;
            TheMediaElement = mediaElement;
            _language = language;

            TheMediaElement.MediaEnded += (s, e) => _mediaReady.Release();

            _model.ApplicationModelUpdate += OnApplicationModelUpdate;
            _ = ConsumeSpeechAsync();
        }

        internal static void AttachNarrator(ApplicationModel model, MediaElement mediaElement, string language)
        {
            var narrator = new Narrator(model, mediaElement, language);
            narrator.Initialize();
        }

        private void Initialize()
        {
            var voiceChoice = new List<VoiceInformation>();
            foreach (var voice in SpeechSynthesizer.AllVoices)
            {
                if (voice.Language.StartsWith(_language))
                {
                    voiceChoice.Add(voice);
                }
            }

            if (voiceChoice.Count != 0)
            {
                _synthesizer.Voice = voiceChoice[0];
            }
        }

        private void OnApplicationModelUpdate(object sender, ApplicationModelUpdateEventArgs e)
        {
            lock (_speech)
            {
                _speech.Enqueue(e);
                Debug.WriteLine($"Sending item {++sent}");
            }
            _semaphore.Release();
        }

        private async Task ConsumeSpeechAsync()
        {
            var received = 0;

            var spokenWords = new List<string>();

            for (; ; )
            {
                ApplicationModelUpdateEventArgs e;

                Debug.WriteLine($"Waiting for {received + 1}");
                await _semaphore.WaitAsync();

                lock (_speech)
                {
                    e = _speech.Dequeue();
                }

                Debug.WriteLine($"Got item {++received}");

                while (!e.IsComplete && await _semaphore.WaitAsync(TimeSpan.FromSeconds(0.25)))
                {
                    lock (_speech)
                    {
                        e = _speech.Dequeue();
                    }

                    Debug.WriteLine($"Immediately replaced with item {++received}");
                }

                var lowWaterMark = 0;
                while (lowWaterMark < spokenWords.Count &&
                    lowWaterMark < e.Words.Count &&
                    spokenWords[lowWaterMark].Equals(e.Words[lowWaterMark], StringComparison.CurrentCultureIgnoreCase))
                {
                    lowWaterMark++;
                }

                if (lowWaterMark == 0)
                {
                    _speechStarted = DateTimeOffset.UtcNow;
                }

                string text;

                if (lowWaterMark < spokenWords.Count)
                {
                    text = "Oops! ";
                    lowWaterMark = 0;
                    spokenWords.Clear();
                }
                else
                {
                    text = string.Empty;
                }

                if (lowWaterMark < e.Words.Count)
                {
                    text += e.Words[lowWaterMark];
                    spokenWords.Add(e.Words[lowWaterMark]);
                    lowWaterMark++;

                    while (lowWaterMark < e.Words.Count)
                    {
                        text += " " + e.Words[lowWaterMark];
                        spokenWords.Add(e.Words[lowWaterMark]);
                        lowWaterMark++;
                    }
                }

                if (e.IsComplete)
                {
                    var speechTime = DateTimeOffset.UtcNow - _speechStarted;
                    var join = string.Join(' ', spokenWords);
                    var trueWordsPerMinute = spokenWords.Count / speechTime.TotalMinutes;
                    var standardWordPerMinute = (join.Length / 5.0) / speechTime.TotalMinutes;
                    var title = $"True wpm = {trueWordsPerMinute:0.0}, standard wpm = {standardWordPerMinute:0.0}";
                    ApplicationView.GetForCurrentView().Title = title;

                    spokenWords.Clear();
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    Debug.WriteLine($"Saying \"{text}\"");

                    Debug.WriteLine("Waiting for media");
                    await _mediaReady.WaitAsync();
                    Debug.WriteLine("Media ready");

                    var stream = await _synthesizer.SynthesizeTextToStreamAsync(text);
                    TheMediaElement.SetSource(stream, stream.ContentType);
                    TheMediaElement.Play();
                }
            }
        }
    }
}
