namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Individual conditional layout.
    /// </summary>
    public class ConditionalLayout
    {
        /// <summary>
        /// The name associated with the conditional layout.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// The optional keys within the layout.
        /// </summary>
        [XmlElement("Gap", typeof(GapKeyLayout))]
        [XmlElement("Char", typeof(CharacterKeyLayout))]
        [XmlElement("Action", typeof(ActionKeyLayout))]
        [XmlElement("Toggle", typeof(ToggleKeyLayout))]
        [XmlElement("State", typeof(StateKeyLayout))]
        public KeyLayout[] Keys { get; set; }

        internal void AssertValid(IKeyboardHost host)
        {
            KeyboardValidationException.Assert(Keys != null, "Keys must be specified");
            KeyboardValidationException.Assert(Keys.Length != 0, "Keys must not be empty");

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
    }
}
