using System;
using System.IO;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    public static class XmlHelper
    {
        /// <summary>
        /// The settings used with <code>XmlWriter</code> instances.
        /// </summary>
        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment
        };

        /// <summary>
        /// The settings used with <code>XmlReader</code> instances.
        /// </summary>
        private static readonly XmlReaderSettings ReaderSettings = new XmlReaderSettings
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

        public static string WriteXmlFragment(Action<XmlWriter> action)
        {
            var output = new StringWriter();
            using (var writer = XmlWriter.Create(output, XmlHelper.WriterSettings))
            {
                action(writer);
            }
            return output.ToString();
        }

        public static T ReadXmlFragment<T>(this string xml, Func<XmlReader, T> action)
        {
            T value;

            var input = new StringReader(xml);
            using (var reader = XmlReader.Create(input, XmlHelper.ReaderSettings))
            {
                value = action(reader);

                ValidateData(!reader.Read());
            }

            return value;
        }
    }
}
