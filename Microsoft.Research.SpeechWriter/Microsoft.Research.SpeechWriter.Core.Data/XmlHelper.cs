using System;
using System.Diagnostics;
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

        internal static bool ReadNodeType(this XmlReader reader, XmlNodeType type)
        {
            reader.ValidateNodeType(type);
            var read = reader.Read();
            return read;
        }

        internal static void ValidatedRead(this XmlReader reader)
        {
            try
            {
                var read = reader.Read();
                ValidateData(read);
            }
            catch
            {
                Debugger.Break();
                throw;
            }
        }

        internal static void ReadEndOfFragment(this XmlReader reader)
        {
            var read = reader.Read();
            ValidateData(!read);
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

                reader.ReadEndOfFragment();
            }

            return value;
        }
    }
}
