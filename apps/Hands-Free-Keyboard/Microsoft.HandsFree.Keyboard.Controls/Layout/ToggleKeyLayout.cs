namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    using System.Xml.Serialization;

    /// <summary>
    /// Toggling key.
    /// </summary>
    public class ToggleKeyLayout : NonGapKeyLayout
    {
        /// <summary>
        /// Name of state toggle attaches to.
        /// </summary>
        [XmlAttribute]
        public string StateName { get; set; }

        /// <summary>
        /// Vocalisation when state set.
        /// </summary>
        [XmlAttribute]
        public string SetVocal { get; set; }

        /// <summary>
        /// Vocalisation when state reset.
        /// </summary>
        [XmlAttribute]
        public string UnsetVocal { get; set; }

        internal override void AssertValid(IKeyboardHost host)
        {
            base.AssertValid(host);

            KeyboardValidationException.Assert(StateName != null, "StateName must be specified");
        }

        internal override void Layout(ILayoutContext context, double left, double top, double width, double height)
        {
            context.CreateToggleKey(this, left, top, width, height);
        }
    }
}
