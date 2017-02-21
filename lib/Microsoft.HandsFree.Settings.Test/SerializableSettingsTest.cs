using Microsoft.HandsFree.Settings.Serialization;
using Microsoft.HandsFree.Settings.Test.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Settings.Test
{
    [TestClass]
    public class SerializableSettingsTest
    {
        [TestMethod]
        public void CreateDefault()
        {
            var ob = SettingsSerializer.CreateDefault<Sample>();
            Assert.AreEqual("Default", ob.Name);
            Assert.AreEqual(1, ob.Value);
            Assert.AreEqual(3.14159, ob.FiddleFactor);
            Assert.IsNotNull(ob.Child);
            Assert.AreEqual(42, ob.Child.Payload);
        }

        [TestMethod]
        public void CreateDefaultAndSave()
        {
            var ob = SettingsSerializer.CreateDefault<Sample>();
            var xml = SettingsSerializer.ToXmlString(ob, "Settings");
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<Settings Name=\"Default\" Value=\"1\" FiddleFactor=\"3.14159\" Problematic=\"0\">\r\n  <Child Payload=\"42\" />\r\n</Settings>", xml);
        }

        [TestMethod]
        public void Read()
        {
            var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><Settings Name=\"Special\" Value=\"12\" FiddleFactor=\"0.5\"><Child Payload=\"1234\" /></Settings>";
            var ob = SettingsSerializer.FromXmlString<Sample>(xml);
            Assert.AreEqual("Special", ob.Name);
            Assert.AreEqual(12, ob.Value);
            Assert.AreEqual(0.5, ob.FiddleFactor);
            Assert.IsNotNull(ob.Child);
            Assert.AreEqual(1234, ob.Child.Payload);
        }

        [TestMethod]
        public void StoredSettings()
        {
            var initialXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<Settings\r\n  UnexpectedAttribute=\"Boo\"\r\n  Name=\"Special\"\r\n  Value=\"12\"\r\n  FiddleFactor=\"0.5\"\r\n  Problematic=\"0\">\r\n  <HelloWorld />\r\n  <Child\r\n    Payload=\"1234\" />\r\n</Settings>";

            var store = SettingsStore<Sample>.CreateFromXml(initialXml);
            var ob = store.Settings;
            Assert.AreEqual("Special", ob.Name);
            Assert.AreEqual(12, ob.Value);
            Assert.AreEqual(0.5, ob.FiddleFactor);
            Assert.IsNotNull(ob.Child);
            Assert.AreEqual(1234, ob.Child.Payload);

            {
                var expected = initialXml;
                var actual = store.ToXmlString();

                Assert.AreEqual(expected, actual);
            }

            {
                store.Settings.Child.Payload = 4321;

                var expected = initialXml.Replace("1234", "4321");
                var actual = store.ToXmlString();

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void CoverProblematic()
        {
            var sample = new Sample();
            sample.Problematic = 42;
            Assert.AreEqual(42, sample.Problematic);
        }
    }
}
