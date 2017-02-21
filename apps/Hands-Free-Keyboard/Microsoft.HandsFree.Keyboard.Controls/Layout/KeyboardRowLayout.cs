using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    /// <summary>
    /// Layout for an individual row of the keyboard.
    /// </summary>
    public class KeyboardRowLayout
    {
        /// <summary>
        /// Height of row as measured in standard key widths.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(1)]
        public double Height { get { return heightField; } set { heightField = value; } }
        double heightField = 1;

        /// <summary>
        /// Width of keys compared to a standard width.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(1)]
        public double KeyWidth { get { return keyWidthField; } set { keyWidthField = value; } }
        double keyWidthField = 1;

        /// <summary>
        /// The keys of the keyboard.
        /// </summary>
        [XmlElement("Gap", typeof(GapKeyLayout))]
        [XmlElement("Char", typeof(CharacterKeyLayout))]
        [XmlElement("Action", typeof(ActionKeyLayout))]
        [XmlElement("Toggle", typeof(ToggleKeyLayout))]
        [XmlElement("State", typeof(StateKeyLayout))]
        [XmlElement("Group", typeof(ConditionalGroupLayout))]
        public KeyLayout[] Keys { get; set; }

        internal void AssertValid(IKeyboardHost host)
        {
            KeyboardValidationException.Assert(Keys != null, "Keys must be specified");
            KeyboardValidationException.Assert(Keys.Length != 0, "Keys must have items");

            foreach (var key in Keys)
            {
                key.AssertValid(host);
            }
        }

        internal void GatherKeyboardStates(ISet<string> states)
        {
            foreach (var key in Keys)
            {
                key.GatherKeyboardStates(states);
            }
        }

        internal double CalculateWidth()
        {
            var width = 0.0;

            foreach (var key in Keys)
            {
                width += key.CalculateWidth();
            }

            return width;
        }

        internal void Layout(ILayoutContext context, double top, double height)
        {
            var x = context.Left;
            foreach (var key in Keys)
            {
                var width = context.KeySize * key.CalculateWidth();
                var y = top + context.KeySize * key.CalculateTopOffset();

                key.Layout(context, x, y, width, height);

                x += width;
            }
        }
    }
}
