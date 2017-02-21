using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    /// <summary>
    /// Single instance of a key.
    /// </summary>
    public abstract class IndividualKeyLayout : KeyLayout
    {
        /// <summary>
        /// Width of key.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(1)]
        public double KeyWidth { get; set; } = 1;

        /// <summary>
        /// Height of key.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(1)]
        public double KeyHeight { get; set; } = 1;

        /// <summary>
        /// Offset from usual position of key top.
        /// </summary>
        [XmlAttribute]
        public double TopOffset { get; set; }

        /// <summary>
        /// Styling.
        /// </summary>
        [XmlAttribute]
        public string Style { get; set; }

        internal override double CalculateWidth()
        {
            return KeyWidth;
        }

        internal override double CalculateTopOffset()
        {
            return TopOffset;
        }
    }
}
