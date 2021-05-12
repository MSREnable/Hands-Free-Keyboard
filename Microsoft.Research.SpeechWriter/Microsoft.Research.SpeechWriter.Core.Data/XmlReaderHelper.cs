using System.IO;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    internal static class XmlReaderHelper
    {
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
