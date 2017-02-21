namespace Microsoft.HandsFree.Prediction.Engine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Class supporting writing and reading of a log file as XML without head or tail decoration.
    /// </summary>
    public class XmlFragmentHelper
    {
        static readonly XmlSerializerNamespaces emptyNamespaces = CreateEmptyNamespaces();

        static readonly XmlWriterSettings omitXmlDeclarationSettings = new XmlWriterSettings { OmitXmlDeclaration = true };

        static readonly Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        static XmlSerializerNamespaces CreateEmptyNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            return namespaces;
        }

        static XmlSerializer GetSerializer<T>()
        {
            var type = typeof(T);

            XmlSerializer serializer;
            if (!serializers.TryGetValue(type, out serializer))
            {
                serializer = new XmlSerializer(type);
                serializers.Add(type, serializer);
            }

            return serializer;
        }

        public static string EncodeXmlFragment<T>(T record)
        {
            var serializer = GetSerializer<T>();

            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, omitXmlDeclarationSettings))
            {
                serializer.Serialize(writer, record, emptyNamespaces);
            }

            var xml = builder.ToString();

            return xml;
        }

        public static T[] DecodeXmlFragments<T>(string xml)
        {
            var serializer = GetSerializer<T[]>();

            T[] records;

            var type = typeof(T);
            var typeName = type.Name;

            var actualXml = "<ArrayOf" + typeName + ">" + xml + "</ArrayOf" + typeName + ">";
            using (var reader = new XmlTextReader(actualXml, XmlNodeType.Element, null))
            {
                var ob = serializer.Deserialize(reader);

                records = (T[])ob;
            }

            return records;
        }

        public static void WriteLog<T>(string path, T entry)
        {
            var xml = EncodeXmlFragment(entry);

            try
            {
                File.AppendAllLines(path, new[] { xml });
            }
            catch (DirectoryNotFoundException)
            {
                var directoryPath = Path.GetDirectoryName(path);
                Directory.CreateDirectory(directoryPath);

                File.AppendAllLines(path, new[] { xml });
            }
        }

        public static T[] ReadLog<T>(string path)
        {
            T[] entries;

            try
            {
                var xml = File.ReadAllText(path);

                entries = DecodeXmlFragments<T>(xml);
            }
            catch
            {
                entries = new T[0];
            }

            return entries;
        }
    }
}
