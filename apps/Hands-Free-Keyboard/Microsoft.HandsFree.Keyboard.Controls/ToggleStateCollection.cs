namespace Microsoft.HandsFree.Keyboard.Controls
{
    using System.Collections.Generic;

    /// <summary>
    /// Collection of toggles to track shift, control, alt states, etc.
    /// </summary>
    public class ToggleStateCollection
    {
        /// <summary>
        /// The toggle states.
        /// </summary>
        readonly Dictionary<string, ToggleState> toggleStates = new Dictionary<string, ToggleState>();

        /// <summary>
        /// Get a toggle state.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ToggleState this[string name]
        {
            get
            {
                ToggleState state;
                if (!toggleStates.TryGetValue(name, out state))
                {
                    state = new ToggleState(name);
                    toggleStates.Add(name, state);
                }

                return state;
            }
        }
    }
}
