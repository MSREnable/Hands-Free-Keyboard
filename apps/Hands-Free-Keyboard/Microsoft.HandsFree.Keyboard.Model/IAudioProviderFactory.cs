using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Interface for audio provider factory.
    /// </summary>
    public interface IAudioProviderFactory
    {
        /// <summary>
        /// Create an audio provider for the given settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The audio provider.</returns>
        IAudioProvider Create(INarrationSettings settings);
    }
}
