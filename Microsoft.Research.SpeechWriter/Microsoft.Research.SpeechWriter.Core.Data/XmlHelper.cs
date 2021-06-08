using System.IO;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    public static class XmlHelper
    {
        /// <summary>
        /// The settings used with <code>XmlWriter</code> instances.
        /// </summary>
        public static XmlWriterSettings WriterSettings { get; } = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        };

        /// <summary>
        /// The settings used with <code>XmlReader</code> instances.
        /// </summary>
        internal static XmlReaderSettings ReaderSettings { get; } = new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment
        };

        internal static void ValidateData(bool truth)
        {
            if (!truth)
            {
                throw new InvalidDataException();
            }
        }

        internal static void ValidateNodeType(this XmlReader reader, XmlNodeType type)
        {
            ValidateData(reader.NodeType == type);
        }

        internal static void ReadNodeType(this XmlReader reader, XmlNodeType type)
        {
            reader.ValidateNodeType(type);
            reader.Read();
        }

        public static string AttributeEscape(this string attribute)
        {
            var value = attribute.Replace("\0", "\\0");
            return value;
        }
    }
}
