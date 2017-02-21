using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Keyboard.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    class AudioProvider : IAudioProvider
    {
        internal const int Null = -2;
        internal const int Default = -1;

        readonly INarrationSettings _settings;

        readonly static IReadOnlyDictionary<AudioTheme, IReadOnlyDictionary<AudioGesture, byte[]>> _audioThemeGestures = CreateAudioThemeGesturesDictionary();

        WavePlayer _player;

        internal AudioProvider(INarrationSettings settings)
        {
            _settings = settings;
        }

        byte[] IAudioProvider.Click => StockAudio.Click;
        byte[] IAudioProvider.BeforeSpeaking => StockAudio.BeforeSpeaking;
        byte[] IAudioProvider.AfterSpeaking => StockAudio.AfterSpeaking;
        byte[] IAudioProvider.RecapCorrection => StockAudio.RecapCorrection;
        byte[] IAudioProvider.RecapSimple => StockAudio.RecapSimple;
        byte[] IAudioProvider.FillerMusic => StockAudio.FillerMusic;

        bool IAudioProvider.IsConnected => AudioProviderFactory.GetMysteryIndex(_settings) != Null;

        static IReadOnlyDictionary<AudioTheme, IReadOnlyDictionary<AudioGesture, byte[]>> CreateAudioThemeGesturesDictionary()
        {
            var male = new Dictionary<AudioGesture, byte[]>();
            male.Add(AudioGesture.Argh, StockAudio.mArgh);
            male.Add(AudioGesture.Cough, StockAudio.mCough);
            male.Add(AudioGesture.Hmm, StockAudio.mHmm);
            male.Add(AudioGesture.Laugh, StockAudio.mLaugh);
            male.Add(AudioGesture.Oh, StockAudio.mOh);
            male.Add(AudioGesture.Sarcasm, StockAudio.mPfft);
            male.Add(AudioGesture.SharpBreath, StockAudio.mSharpBreath);
            male.Add(AudioGesture.Ugh, StockAudio.mUgh);

            var female = new Dictionary<AudioGesture, byte[]>();
            female.Add(AudioGesture.Argh, StockAudio.fArgh);
            female.Add(AudioGesture.Cough, StockAudio.fCough);
            female.Add(AudioGesture.Hmm, StockAudio.fHmm);
            female.Add(AudioGesture.Laugh, StockAudio.fLaugh);
            female.Add(AudioGesture.Oh, StockAudio.fOh);
            female.Add(AudioGesture.Sarcasm, StockAudio.fPfft);
            female.Add(AudioGesture.SharpBreath, StockAudio.fSharpBreath);
            female.Add(AudioGesture.Ugh, StockAudio.fUgh);

            var dictionary = new Dictionary<AudioTheme, IReadOnlyDictionary<AudioGesture, byte[]>>();
            dictionary.Add(AudioTheme.Female, female);
            dictionary.Add(AudioTheme.Male, male);

            return dictionary;
        }

        byte[] IAudioProvider.GetAudioGesture(AudioTheme theme, AudioGesture gesture)
        {
            byte[] bytes = _audioThemeGestures[theme][gesture];
            return bytes;
        }

        async Task IAudioProvider.PlayAsync(byte[] buffer, double volume)
        {
            var index = AudioProviderFactory.GetMysteryIndex(_settings);

            if (index != Null)
            {
                _player = WavePlayer.Play(index, buffer, volume);
                await _player.WaitAsync();
                _player = null;
            }
        }

        async Task IAudioProvider.PlaySilenceAsync(TimeSpan timeSpan)
        {
            _player = WavePlayer.PlaySilence(timeSpan);
            await _player.WaitAsync();
            _player = null;
        }

        void IAudioProvider.Stop()
        {
            var player = _player;
            player?.Stop();
        }

        void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
