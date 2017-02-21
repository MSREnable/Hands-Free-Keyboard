using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    /// <summary>
    /// Representation of a keyboard layout.
    /// </summary>
    public class KeyboardLayout
    {
        /// <summary>
        /// Class XML serializer.
        /// </summary>
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(KeyboardLayout));

        static void HandleUnknown(object sender, EventArgs e, string name, int lineNumber, int linePosition, string typeName)
        {
            var message = $"Unknown {typeName} at line {lineNumber} position {linePosition}: {name}";
            Debug.Fail(message);
        }

        static KeyboardLayout()
        {
            Serializer.UnknownNode += (s, e) => Debug.Fail($"Unknown {e.Name} at {e.LineNumber}:{e.LinePosition}");
            Serializer.UnreferencedObject += (s, e) => Debug.Fail("Unrefferenced object in XML");
        }

        /// <summary>
        /// Width of the keyboard in key widths.
        /// </summary>
        [XmlIgnore]
        public double KeyWidth { get { return Rows[0].CalculateWidth(); } }

        /// <summary>
        /// Height of the keyboard in key widths (sic).
        /// </summary>
        [XmlIgnore]
        public double KeyHeight
        {
            get
            {
                var height = 0.0;
                foreach (var row in Rows)
                {
                    height += row.Height;
                }
                return height;
            }
        }

        /// <summary>
        /// Font size for control.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(24)]
        public double FontSize { get { return fontSizeField; } set { fontSizeField = value; } }
        double fontSizeField = 24;

        /// <summary>
        /// The rows of the keyboard.
        /// </summary>
        [XmlElement("Row")]
        public KeyboardRowLayout[] Rows { get; set; }

        /// <summary>
        /// Validate the layout.
        /// </summary>
        public void AssertValid(IKeyboardHost host)
        {
            KeyboardValidationException.Assert(Rows != null, "Rows must be specified");
            KeyboardValidationException.Assert(Rows.Length != 0, "One or more rows");

            Rows[0].AssertValid(host);
            var row0Width = Rows[0].CalculateWidth();

            for (var rowIndex = 1; rowIndex < Rows.Length; rowIndex++)
            {
                var row = Rows[rowIndex];
                row.AssertValid(host);
                var rowWidth = row.CalculateWidth();

                KeyboardValidationException.Assert(row0Width == rowWidth, "All rows must be same length");
            }
        }

        /// <summary>
        /// Identify all the states specified in the keyboard layout.
        /// </summary>
        /// <param name="states"></param>
        public void GatherKeyboardStates(ISet<string> states)
        {
            foreach (var row in Rows)
            {
                row.GatherKeyboardStates(states);
            }
        }

        /// <summary>
        /// Layout the keyboard onto a Canvas.
        /// </summary>
        /// <param name="context">The layout context.</param>
        internal void Layout(ILayoutContext context)
        {
            var y = context.Top;
            foreach (var row in Rows)
            {
                var height = row.Height * context.KeySize;

                row.Layout(context, y, height);

                y += height;
            }
        }

        /// <summary>
        /// Save the layout to an XML string.
        /// </summary>
        /// <returns>The XML equivalent.</returns>
        public string Save()
        {
            var stream = new MemoryStream();
            var writer = new XmlTextWriter(stream, Encoding.UTF8)
            {
                Formatting = Formatting.Indented
            };
            Serializer.Serialize(writer, this);
            var bytes = stream.ToArray();
            var text = Encoding.UTF8.GetString(bytes);

            // Remove the BOM.
            if (text[0] == '\xFEFF')
            {
                text = text.Substring(1);
            }

            return text;
        }

        /// <summary>
        /// Create layout from XML.
        /// </summary>
        /// <param name="keyboardXml">The XML.</param>
        /// <returns>Layout object.</returns>
        public static KeyboardLayout Load(string keyboardXml)
        {
            object ob;
            var stringReader = new StringReader(keyboardXml);
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                ob = Serializer.Deserialize(xmlReader);
            }

            var layout = (KeyboardLayout)ob;
            return layout;
        }
    }
}
