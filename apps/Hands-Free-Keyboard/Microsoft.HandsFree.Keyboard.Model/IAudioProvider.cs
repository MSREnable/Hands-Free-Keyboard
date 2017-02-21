using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.HandsFree.Keyboard.Model
{
    public interface IAudioProvider : IDisposable
    {
        /// <summary>
        /// Key press feedback sound.
        /// </summary>
        byte[] Click { get; }

        /// <summary>
        /// Sound before starting to play audio for an utterance.
        /// </summary>
        byte[] BeforeSpeaking { get; }

        /// <summary>
        /// Sound after playing audio for an utterance.
        /// </summary>
        byte[] AfterSpeaking { get; }

        /// <summary>
        /// Sound played before recapping partial utterance after a correcting edit.
        /// </summary>
        byte[] RecapCorrection { get; }

        /// <summary>
        /// Sound played before recapping partial utterance after a delay.
        /// </summary>
        byte[] RecapSimple { get; }

        /// <summary>
        /// Sound to play during delays between other audio feedback.
        /// </summary>
        byte[] FillerMusic { get; }

        /// <summary>
        /// Is this a real device.
        /// </summary>
        bool IsConnected { get; }

        byte[] GetAudioGesture(AudioTheme theme, AudioGesture gesture);

        Task PlayAsync(byte[] buffer, double volume);

        Task PlaySilenceAsync(TimeSpan timeSpan);

        void Stop();
    }
}
