using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Settings.Test
{
    [TestClass]
    public class SerializationTest
    {
        const string SimpleOldSettingsExampleAsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<Settings AlwaysHere=\"1\" LegacyValue=\"2\" />";
        const string SimpleNewSettingsExampleAsXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<Settings AlwaysHere=\"1\" IntroducedValue=\"3\" />";

        static void RoundTripXml(string expected)
        {
            var document = new XmlDocument();

            var reader = new StringReader(expected);
            document.Load(reader);

            var writer = new StringWriter();
            document.Save(writer);

            var actual = writer.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SimpleSerializeOldTest()
        {
            var settings = new OldSettings { AlwaysHere = 1, LegacyValue = 2 };

            var writer = new StringWriter();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            OldSettings.Serializer.Serialize(writer, settings, namespaces);

            var xml = writer.ToString();
            Assert.AreEqual(SimpleOldSettingsExampleAsXml, xml);

            RoundTripXml(xml);
        }
        [TestMethod]
        public void SimpleSerializeNewTest()
        {
            var settings = new NewSettings { AlwaysHere = 1, IntroducedValue = 3 };

            var writer = new StringWriter();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            NewSettings.Serializer.Serialize(writer, settings, namespaces);

            var xml = writer.ToString();
            Assert.AreEqual(SimpleNewSettingsExampleAsXml, xml);

            RoundTripXml(xml);
        }

        [TestMethod]
        public void CreateXDocumentTest()
        {
            var document = new XmlDocument();
            var reader = new StringReader(SimpleOldSettingsExampleAsXml);
            document.Load(reader);
        }
    }
}
