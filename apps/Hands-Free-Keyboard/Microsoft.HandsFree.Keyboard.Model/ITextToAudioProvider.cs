using System;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Interface for text to audio.
    /// </summary>
    public interface ITextToAudioProvider : IDisposable
    {
        /// <summary>
        /// Convert text to audio.
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        byte[] ToAudio(Voice voice, string text);

        AudioTheme GetAudioTheme(Voice voice);
    }
}
