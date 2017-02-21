using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    /// <summary>
    /// Keyboard layout switching key.
    /// </summary>
    public class StateKeyLayout : NonGapKeyLayout
    {
        /// <summary>
        /// Name of state toggle attaches to.
        /// </summary>
        [XmlAttribute]
        public string StateName { get; set; }

        /// <summary>
        /// Vocal version of state.
        /// </summary>
        [XmlAttribute]
        public string Vocal { get; set; }

        internal override void GatherKeyboardStates(ISet<string> states)
        {
            base.GatherKeyboardStates(states);

            if (StateName != null)
            {
                states.Add(StateName);
            }
        }

        internal override void Layout(ILayoutContext context, double left, double top, double width, double height)
        {
            context.CreateStateKey(this, left, top, width, height);
        }
    }
}
