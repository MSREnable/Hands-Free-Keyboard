using Microsoft.HandsFree.Keyboard.Model;

namespace Microsoft.HandsFree.Keyboard.Model
{
    abstract class Audio
    {
        internal abstract void Play(IAudioProvider provider);
    }
}
