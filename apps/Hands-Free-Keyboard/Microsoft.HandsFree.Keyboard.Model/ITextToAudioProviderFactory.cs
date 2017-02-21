using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Interface for text to audio provider factory.
    /// </summary>
    public interface ITextToAudioProviderFactory
    {
        /// <summary>
        /// Create a new instance of a text to audio provider.
        /// </summary>
        /// <returns>The provider.</returns>
        ITextToAudioProvider Create(INarrationSettings settings);
    }
}
