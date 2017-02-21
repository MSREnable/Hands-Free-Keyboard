namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    using System.Collections.Generic;

    /// <summary>
    /// List of controls to show and hide for a state.
    /// </summary>
    public

    class StateShowHideContainer<TControl>
    {
        /// <summary>
        /// Controls to show for state.
        /// </summary>
        public List<TControl> ShowList { get { return showListField; } }
        readonly List<TControl> showListField = new List<TControl>();

        /// <summary>
        /// Controls to hide for state.
        /// </summary>
        public List<TControl> HideList { get { return hideListField; } }
        readonly List<TControl> hideListField = new List<TControl>();
    }
}
