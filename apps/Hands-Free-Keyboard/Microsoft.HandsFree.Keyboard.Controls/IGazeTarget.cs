namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Target for gaze activation.
    /// </summary>
    public interface IGazeTarget
    {
        /// <summary>
        /// The keystrokes rendered by the control, or null if the key does not render strokes.
        /// </summary>
        string SendKeys { get; }

        /// <summary>
        /// Multiplier applied to time calculation.
        /// </summary>
        double Multiplier { get; }

        /// <summary>
        /// Multiplier applied to repetition time calculation.
        /// </summary>
        double RepeatMultiplier { get; }
    }
}
