using System.IO;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    internal static class XmlReaderHelper
    {
        /// <summary>
        /// The settings used with <code>XmlWriter</code> instances.
        /// </summary>
        internal static XmlWriterSettings WriterSettings { get; } = new XmlWriterSettings
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
    }
}
