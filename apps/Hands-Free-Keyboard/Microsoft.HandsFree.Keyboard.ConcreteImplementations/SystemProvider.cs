using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Win32;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    /// <summary>
    /// ISystemProvider implementation.
    /// </summary>
    public class SystemProvider : ISystemProvider
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static readonly ISystemProvider Instance = new SystemProvider();

        /// <summary>
        /// Private constructor.
        /// </summary>
        SystemProvider()
        {
        }

        /// <summary>
        /// Is the system shift key down.
        /// </summary>
        public bool IsShiftKeyDown { get { return (User32.GetKeyState(User32.VK_SHIFT) & 0x80) != 0; } }

        /// <summary>
        /// Is the system control key down.
        /// </summary>
        public bool IsControlKeyDown { get { return (User32.GetKeyState(User32.VK_CONTROL) & 0x80) != 0; } }

        /// <summary>
        /// Is the system alt key down.
        /// </summary>
        public bool IsAltKeyDown { get { return (User32.GetKeyState(User32.VK_ALT) & 0x80) != 0; } }
    }
}
