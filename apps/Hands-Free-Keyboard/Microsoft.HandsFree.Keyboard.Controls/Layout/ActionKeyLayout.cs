namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    using System.Xml.Serialization;

    /// <summary>
    /// Action causing key.
    /// </summary>
    public class ActionKeyLayout : NonGapKeyLayout
    {
        /// <summary>
        /// Name of action to perform.
        /// </summary>
        [XmlAttribute]
        public string Action { get; set; }

        /// <summary>
        /// The vocalisation of the key.
        /// </summary>
        [XmlAttribute]
        public string Vocal { get; set; }

        internal override void AssertValid(IKeyboardHost host)
        {
            base.AssertValid(host);

            KeyboardValidationException.Assert(Action != null, "Action must be specified");
            KeyboardValidationException.Assert(host.GetAction(Action) != null, "Action must be a valid action name: {0}", Action);
        }

        internal override void Layout(ILayoutContext context, double left, double top, double width, double height)
        {
            context.CreateActionKey(this, left, top, width, height);
        }
    }
}
