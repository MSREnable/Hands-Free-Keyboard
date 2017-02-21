namespace Microsoft.HandsFree.Keyboard.Controls
{
    using System;

    /// <summary>
    /// Class representing a toggle state.
    /// </summary>
    public class ToggleState
    {
        ToggleState(string name, bool initialState)
        {
            Name = name;
            IsChecked = initialState;
        }

        internal ToggleState(string name)
            : this(name, false)
        {
        }

        /// <summary>
        /// Name of toggle state.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the current check state.
        /// </summary>
        public bool IsChecked
        {
            get { return _isCheckedField; }
            set
            {
                if (_isCheckedField != value)
                {
                    _isCheckedField = value;

                    CheckChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        bool _isCheckedField;

        /// <summary>
        /// Event raised when check state changes.
        /// </summary>
        public event EventHandler CheckChanged;
    }
}
