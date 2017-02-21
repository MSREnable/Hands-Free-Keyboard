namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Xml.Serialization;

    /// <summary>
    /// Key that is an actual key.
    /// </summary>
    public abstract class NonGapKeyLayout : IndividualKeyLayout
    {
        /// <summary>
        /// Standard keytop caption.
        /// </summary>
        [XmlAttribute]
        public string Caption { get; set; }

        /// <summary>
        /// The font size to render the caption in.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0)]
        public double FontSize { get; set; }

        /// <summary>
        /// Multiplier value for controlling gaze duration.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(1)]
        public double Multiplier { get; set; } = 1;

        /// <summary>
        /// Repeat delay multiplier.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0.0)]
        public double RepeatMultiplier { get; set; }

        internal override void AssertValid(IKeyboardHost host)
        {
            base.AssertValid(host);

            KeyboardValidationException.Assert(Caption != null, "Caption must be specified");
        }
    }
}
