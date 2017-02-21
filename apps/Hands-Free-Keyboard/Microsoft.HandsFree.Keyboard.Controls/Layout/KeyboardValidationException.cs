namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    using System;

    /// <summary>
    /// Layout validation exception.
    /// </summary>
    [Serializable]
    public class KeyboardValidationException : Exception
    {
        KeyboardValidationException(string message)
            : base(message)
        {
        }

        internal static void Assert(bool assertion, string format, params string[] args)
        {
            if (!assertion)
            {
                var exception = new KeyboardValidationException(string.Format(format, args));
                throw exception;
            }
        }
    }
}
