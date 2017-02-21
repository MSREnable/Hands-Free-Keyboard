using Microsoft.HandsFree.Prediction.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.Model
{
    class Narrator : INarrator
    {
        static readonly string[] _mutterWords = new[] { "cloff", "prunk", "shote", "cucking", "fusk", "pempslider", "pim-hole" };

        readonly INarrationSettings _settings;

        readonly IAudioProvider _audioProvider;

        readonly ITextToAudioProvider _textToAudioProvider;

        readonly IActivityDisplayProvider _activityDisplayProvider;

        readonly List<byte[]> _speechQueue = new List<byte[]>();

        /// <summary>
        /// Are we actively speeking.
        /// </summary>
        bool _isActivelySpeaking;

        /// <summary>
        /// Are we currently speaking?
        /// </summary>
        bool _isSpeaking;

        /// <summary>
        /// Time speaking went idle.
        /// </summary>
        DateTime _speakingIdleTime = DateTime.UtcNow;

        /// <summary>
        /// The previously spoken partial sentence.
        /// </summary>
        string _previousPartial = string.Empty;

        /// <summary>
        /// Are we filling time by playing background music?
        /// </summary>
        bool _isPlayingFiller;

        /// <summary>
        /// The audio task.
        /// </summary>
        Task _audioTask;

        /// <summary>
        /// Guard to stop re-entrancy of audio. TODO: Move audio logic to UI thread to avoid this.
        /// </summary>
        readonly SemaphoreSlim _reentrancySemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// The audio for echoing.
        /// </summary>
        byte[] _echoBuffer;

        /// <summary>
        /// Next filler audio.
        /// </summary>
        byte[] _fillerBuffer;

        /// <summary>
        /// Source of randomness.
        /// </summary>
        readonly Random _random;

        /// <summary>
        /// Index of last word muttered, used to avoid muttering the same word twice in succession.
        /// </summary>
        int _lastMutterance;

        /// <summary>
        /// The most recently queued sentence to be spoken, the sentence being spoken or null if no
        /// sentences are current.
        /// </summary>
        string _currentSentence;

        /// <summary>
        /// If _currentSentence is non-null, then this will be 0 if the sentence is currently being
        /// spoken or positive to indicate the number of audio buffers that will be played before
        /// the sentence starts being spoken.
        /// </summary>
        int _currentSentencePosition;

        internal Narrator(Random random, INarrationSettings settings, IAudioProviderFactory factory, ITextToAudioProvider textToAudioProvider, IActivityDisplayProvider activityDisplayProvider)
        {
            _settings = settings;
            _audioProvider = factory.Create(settings);
            _textToAudioProvider = textToAudioProvider;
            _random = random;

            _activityDisplayProvider = activityDisplayProvider;
        }

        static string Cheedleaderify(string vocal)
        {
            {
                switch (char.ToLowerInvariant(vocal[0]))
                {
                    case 'a':
                    case 'e':
                    case 'f':
                    case 'h':
                    case 'i':
                    case 'l':
                    case 'n':
                    case 'm':
                    case 'o':
                    case 'r':
                    case 'u':
                    case 'x':
                        vocal = "Give me an " + vocal;
                        break;

                    default:
                        vocal = "Give me a " + vocal;
                        break;
                }
            }

            return vocal;
        }

        /// <summary>
        /// Speak some text.
        /// </summary>
        /// <param name="voice">The voice to use for speaking the text.</param>
        /// <param name="vocal">The text to be spoken,</param>
        /// <returns>The number of items queue to be played before this. 0 means it
        /// will start playing immediately, -1 that something has failed.</returns>
        int PlayFeedback(Voice voice, string vocal)
        {
            int position;

            Debug.Assert(vocal != null);

            _activityDisplayProvider.IsSpeaking = true;

            if (!_isActivelySpeaking)
            {
                PlayFeedbackSound(_audioProvider.BeforeSpeaking);
                _isActivelySpeaking = true;
            }

            if (voice == Voice.Letter && _settings.IsCheerleaderMode && vocal.Length != 0)
            {
                vocal = Cheedleaderify(vocal);
            }

            var buffer = _textToAudioProvider.ToAudio(voice, vocal);

            if (buffer != null)
            {
                position = PlayFeedback(buffer);
            }
            else
            {
                position = -1;
            }

            var echoLength = vocal.ReverseWordLength(vocal.Length);
            if (echoLength == vocal.Length)
            {
                _echoBuffer = buffer;
            }
            else
            {
                var echoWord = vocal.Substring(vocal.Length - echoLength);
                _echoBuffer = _textToAudioProvider.ToAudio(voice, echoWord);
            }

            return position;
        }

        void PlayUnconditionalFeedbackSound(byte[] buffer)
        {
            PlayFeedback(buffer);
        }

        void PlayFeedbackSound(byte[] buffer)
        {
            if (_settings.PlaySoundEffects)
            {
                PlayUnconditionalFeedbackSound(buffer);
            }
        }

        /// <summary>
        /// Queue an audio buffer to be played.
        /// </summary>
        /// <param name="buffer">The buffer to play.</param>
        /// <returns>The number of items queued to play before the buffer.</returns>
        int PlayFeedback(byte[] buffer)
        {
            _reentrancySemaphore.Wait();

            if (_isPlayingFiller)
            {
                _isPlayingFiller = false;
                _audioProvider.Stop();
            }

            if (_audioTask == null)
            {
                Debug.Assert(_speechQueue.Count == 0, "Queue should be empty");

                // Speak the text.
                _isSpeaking = true;
                _audioTask = _audioProvider.PlayAsync(buffer, 1);
                _audioTask.ContinueWith(OnCompleted);
            }
            else
            {
                _speechQueue.Add(buffer);
            }

            var position = _speechQueue.Count;

            _reentrancySemaphore.Release();

            return position;
        }

        byte[] GetNextSilenceFiller()
        {
            byte[] filler;
            switch (_settings.SilenceFiller)
            {
                case SilenceFiller.Music:
                    filler = _audioProvider.FillerMusic;
                    break;

                case SilenceFiller.Echo:
                    filler = _echoBuffer;
                    break;

                case SilenceFiller.Muttering:
                    var index = _random.Next(_mutterWords.Length - 1);
                    if (index == _lastMutterance)
                    {
                        index = _mutterWords.Length - 1;
                    }
                    _lastMutterance = index;
                    filler = _textToAudioProvider.ToAudio(Voice.Word, _mutterWords[index]);
                    break;

                default:
                    Debug.Fail("Should not arrive here");
                    filler = null;
                    break;
            }

            return filler;
        }

        void OnCompleted(Task t)
        {
            _reentrancySemaphore.Wait();

            // If there is a sentence to be spoken...
            if (_currentSentence != null)
            {
                // ...and we are speaking it...
                if (_currentSentencePosition == 0)
                {
                    // ...there is no longer a sentence to be spoken...
                    _currentSentence = null;
                }
                else
                {
                    // ...otherwise it is one step closer to being spoken.
                    _currentSentencePosition--;
                }
            }

            if (_speechQueue.Count != 0)
            {
                var item = _speechQueue[0];
                _speechQueue.RemoveAt(0);

                _audioTask = _audioProvider.PlayAsync(item, 1);
                _audioTask.ContinueWith(OnCompleted);
            }
            else if (!_isPlayingFiller)
            {
                _activityDisplayProvider.IsSpeaking = false;

                _isSpeaking = false;
                _speakingIdleTime = DateTime.UtcNow;

                if (_activityDisplayProvider.IsTyping && _audioProvider.IsConnected && _settings.SilenceFiller != SilenceFiller.None)
                {
                    var filler = GetNextSilenceFiller();

                    _isPlayingFiller = true;
                    if (_settings.SilenceFillerDelay == 0)
                    {
                        _audioTask = _audioProvider.PlayAsync(filler, _settings.SilenceFillerVolume / 100.0);
                    }
                    else
                    {
                        _audioTask = _audioProvider.PlaySilenceAsync(TimeSpan.FromMilliseconds(_settings.SilenceFillerDelay));
                    }
                    _audioTask.ContinueWith(OnCompleted);

                    _fillerBuffer = GetNextSilenceFiller();
                }
                else
                {
                    _audioTask = null;
                }
            }
            else if (_settings.SilenceFiller != SilenceFiller.None)
            {
                Debug.Assert(_isPlayingFiller);

                _audioTask = _audioProvider.PlayAsync(_fillerBuffer, _settings.SilenceFillerVolume / 100.0);
                _fillerBuffer = GetNextSilenceFiller();
                _audioTask.ContinueWith(OnCompleted);
            }
            else
            {
                _isPlayingFiller = false;
                _audioTask = null;
            }

            _reentrancySemaphore.Release();
        }

        bool IsRecentlyIdle
        {
            get
            {
                var idleTime = DateTime.UtcNow - _speakingIdleTime;
                var idleThreshold = TimeSpan.FromSeconds(_settings.SentenceRecapThreshold);
                return idleThreshold.Ticks == 0 || idleTime < idleThreshold;
            }
        }

        void PlayFeedbackWord(string partialSentence, string vocal)
        {
            if (!partialSentence.StartsWith(_previousPartial))
            {
                if (_settings.ReadCompletedWords)
                {
                    PlayFeedbackSound(_audioProvider.RecapCorrection);
                }

                PlayFeedback(Voice.Word, partialSentence);
            }
            else if (_isSpeaking || IsRecentlyIdle)
            {
                PlayFeedback(Voice.Word, vocal);
            }
            else
            {
                if (_isActivelySpeaking && _settings.ReadCompletedWords)
                {
                    PlayFeedbackSound(_audioProvider.RecapSimple);
                }
                PlayFeedback(Voice.Word, partialSentence);
            }

            _previousPartial = partialSentence;
        }

        void PlayFeedbackFixedSentence(string vocal)
        {
            PlayFeedback(Voice.Sentence, vocal);
        }

        void PlayFeedbackSentence(string vocal)
        {
            // Only speak a new sentence or if we're not in suppress duplicate sentences mode.
            if (vocal != _currentSentence || _settings.SpeechConcurrencyBehavior != SpeechConcurrencyBehavior.SuppressDuplicates)
            {
                var positon = PlayFeedback(Voice.Sentence, vocal);

                if (positon != -1)
                {
                    _currentSentence = vocal;
                    _currentSentencePosition = positon;
                }
            }
        }

        bool PlayFeedbackComposedSentence(string keyLabel, string vocal, bool isRepeat)
        {
            var spoken = false;

            switch (_settings.SentenceBehavior)
            {
                case SentenceBehavior.Always:
                default:
                    PlayFeedbackSentence(vocal);
                    spoken = true;
                    break;

                case SentenceBehavior.OnlyRepetition:
                    if (isRepeat)
                    {
                        PlayFeedbackSentence(vocal);
                        spoken = true;
                    }
                    break;

                case SentenceBehavior.Command:
                    if (_settings.ReadKeyTops)
                    {
                        PlayFeedback(Voice.Word, keyLabel);
                        spoken = true;
                    }
                    break;
            }

            if (_isActivelySpeaking)
            {
                PlayFeedbackSound(_audioProvider.AfterSpeaking);
                _isActivelySpeaking = false;
            }

            _previousPartial = string.Empty;

            return spoken;
        }

        void Reset()
        {
            _isActivelySpeaking = false;

            _activityDisplayProvider.IsTyping = false;

            _previousPartial = string.Empty;
        }

        void CancelPendingSound()
        {
            _reentrancySemaphore.Wait();

            _speechQueue.Clear();
            _currentSentence = null;
            _currentSentencePosition = 0;

            _isPlayingFiller = false;
            _audioProvider.Stop();

            _reentrancySemaphore.Release();
        }

        void INarrator.OnNarrationEvent(NarrationEventArgs e)
        {
            var spoken = false;

            if (e.EventType != NarrationEventType.GotSuggestion)
            {
                _activityDisplayProvider.IsTyping = (e.EventType == NarrationEventType.SimpleTyping || !e.IsRepeat)
                    && (e.EventType != NarrationEventType.Utter || e.EventType == NarrationEventType.Reset);

                if (e.CompletedWord != null && _settings.ReadCompletedWords)
                {
                    PlayFeedbackWord(e.Utterance.Substring(0, e.CursorPosition), e.CompletedWord);
                    spoken = true;
                }
            }

            switch (e.EventType)
            {
                case NarrationEventType.VocalGesture:
                    if (_settings.PlayVocalGestures)
                    {
                        CancelPendingSound();

                        var theme = _textToAudioProvider.GetAudioTheme(Voice.Sentence);
                        var gesture = _audioProvider.GetAudioGesture(theme, e.Gesture);
                        PlayUnconditionalFeedbackSound(gesture);

                        spoken = true;
                    }
                    break;

                case NarrationEventType.SimpleTyping:
                case NarrationEventType.Simple:
                    if (_settings.ReadKeyTops)
                    {
                        PlayFeedback(Voice.Letter, e.KeyTop);
                        spoken = true;
                    }
                    break;

                case NarrationEventType.WordCompletion:
                    if (_settings.ReadKeyTops && !spoken)
                    {
                        PlayFeedback(Voice.Letter, e.KeyTop);
                        spoken = true;
                    }
                    break;

                case NarrationEventType.Reset:
                    if (_settings.ReadKeyTops)
                    {
                        PlayFeedback(Voice.Letter, e.KeyTop);
                        spoken = true;
                    }
                    Reset();
                    break;

                case NarrationEventType.Utter:
                    spoken |= PlayFeedbackComposedSentence(e.KeyTop, e.Utterance, e.IsRepeat);
                    break;

                case NarrationEventType.FixedUtter:
                    if (_settings.SentenceBehavior != SentenceBehavior.Command)
                    {
                        PlayFeedbackFixedSentence(e.KeyTop);
                    }
                    break;

                case NarrationEventType.AcceptSuggestion:
                    if (_settings.ReadKeyTops && !spoken)
                    {
                        PlayFeedback(Voice.Letter, e.CompletedWord);
                        spoken = true;
                    }
                    break;

                case NarrationEventType.GotSuggestion:
                    if (_settings.ReadTopSuggestion)
                    {
                        PlayFeedback(Voice.Letter, e.Suggestion);
                    }
                    spoken = true;
                    break;

                default:
                    Debug.Fail($"Unknown event type {e.EventType}");
                    break;
            }

            if (!spoken && _settings.IsClickOn)
            {
                PlayUnconditionalFeedbackSound(_audioProvider.Click);
            }
        }
    }
}
