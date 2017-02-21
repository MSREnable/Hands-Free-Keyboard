namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Interface to system function.
    /// </summary>
    public interface ISystemProvider
    {
        /// <summary>
        /// Is the system shift key down.
        /// </summary>
        bool IsShiftKeyDown { get; }

        /// <summary>
        /// Is the system control key down.
        /// </summary>
        bool IsControlKeyDown { get; }

        /// <summary>
        /// Is the system alt key down.
        /// </summary>
        bool IsAltKeyDown { get; }
    }
}
