using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

[assembly: InternalsVisibleTo("Microsoft.Research.SpeechWriter.Core.Data.Test")]

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    public class TileData
    {
        private static readonly string[] _elementNames = new[] { "T", "B", "A", "J" };

        public TileData(string content)
            : this(content, isGlueBefore: false, isGlueAfter: false)
        {
        }

        public TileData(string content, bool isGlueBefore = false, bool isGlueAfter = false)
        {
            Content = content;
            IsGlueBefore = isGlueBefore;
            IsGlueAfter = isGlueAfter;

            Debug.Assert(!IsSpaces || (IsGlueBefore & IsGlueAfter), "Spaces must always be glued both sides");
        }

        public string Content { get; }

        private bool IsNoGlue => !IsGlueBefore && !IsGlueAfter;

        /// <summary>
        /// Attach without space to the previous item.
        /// </summary>
        [XmlIgnore]
        public bool IsGlueAfter { get; }

        /// <summary>
        /// Attach without space to the next item.
        /// </summary>
        [XmlIgnore]
        public bool IsGlueBefore { get; }

        [XmlIgnore]
        internal bool IsSimpleWord => IsNoGlue && 0 < Content.Length && char.IsLetterOrDigit(Content[0]) && char.IsLetterOrDigit(Content[Content.Length - 1]) &&
            !Content.Contains(" ");

        [XmlIgnore]
        internal bool IsSpaces => Content.Replace(" ", string.Empty) == string.Empty;

        internal void ToXmlWriter(XmlWriter writer, bool isElemental)
        {
            if (isElemental)
            {
                var nameIndex = (IsGlueAfter ? 1 : 0) + (IsGlueBefore ? 2 : 0);
                var localName = _elementNames[nameIndex];
                writer.WriteStartElement(localName);
            }
            writer.WriteString(Content);
            if (isElemental)
            {
                writer.WriteEndElement();
            }
        }

        internal static TileData FromXmlReader(XmlReader reader)
        {
            reader.ValidateNodeType(XmlNodeType.Element);
            var nameIndex = Array.IndexOf(_elementNames, reader.Name);
            XmlReaderHelper.ValidateData(0 <= nameIndex);
            reader.Read();

            reader.ValidateNodeType(XmlNodeType.Text);
            var text = reader.Value;
            var tile = new TileData(content: text, isGlueBefore: (nameIndex & 2) != 0, isGlueAfter: (nameIndex & 1) != 0);

            reader.Read();
            reader.ReadNodeType(XmlNodeType.EndElement);

            return tile;
        }

        public bool Equals(TileData element)
        {
            var value = Content == element.Content &&
                IsGlueBefore == element.IsGlueBefore &&
                IsGlueAfter == element.IsGlueAfter;
            return value;
        }

        public override bool Equals(object obj)
        {
            var element = obj as TileData;
            return element != null && Equals(element);
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode() ^ IsGlueBefore.GetHashCode() ^ IsGlueAfter.GetHashCode();
        }

        public override string ToString()
        {
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };
            var output = new StringWriter();
            using (var writer = XmlWriter.Create(output, settings))
            {
                ToXmlWriter(writer, true);
            }
            var value = output.ToString();
            return value;
        }
    }
}
