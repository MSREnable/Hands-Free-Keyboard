using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Class of an object that will narrate what is happening in an ApplicationModel.
    /// </summary>
    public class Narrator
    {
        private readonly ApplicationModel _model;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        private readonly Queue<ApplicationModelUpdateEventArgs> _speech = new Queue<ApplicationModelUpdateEventArgs>();

        private int sent;

        private DateTimeOffset _speechStarted;

        private readonly INarratorVocalizer _vocalizer;

        private Narrator(ApplicationModel model, INarratorVocalizer vocalizer)
        {
            _model = model;

            _vocalizer = vocalizer;

            _model.ApplicationModelUpdate += OnApplicationModelUpdate;
            _ = ConsumeSpeechAsync();
        }

        /// <summary>
        /// Attach a narrator to an ApplicationModel.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="vocalizer"></param>
        public static Narrator AttachNarrator(ApplicationModel model, INarratorVocalizer vocalizer)
        {
            var narrator = new Narrator(model, vocalizer);
            return narrator;
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
                    _vocalizer.DisplayWordPerMinuteEstimate(spokenWords, speechTime);

                    spokenWords.Clear();
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    Debug.WriteLine($"Saying \"{text}\"");

                    if (_model.Environment.Settings.SpeakWordByWord)
                    {
                        await _vocalizer.SpeakSsmlAsync(text);
                    }
                }
            }
        }
    }
}
