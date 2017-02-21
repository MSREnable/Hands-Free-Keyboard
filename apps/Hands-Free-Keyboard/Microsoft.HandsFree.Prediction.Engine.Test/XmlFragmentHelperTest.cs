namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;

    [TestClass]
    public class XmlFragmentHelperTest
    {
        [TestMethod]
        public void SerializeXmlElement()
        {
            var ob = new DemoPayload { Text = "Some Text", Number = 42 };

            var serializer = new XmlSerializer(ob.GetType());

            var builder = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };

            using (var xmlWriter = XmlWriter.Create(builder, settings))
            {
                serializer.Serialize(xmlWriter, ob, namespaces);
            }

            var xml = builder.ToString();

            Assert.AreEqual("<DemoPayload Text=\"Some Text\" Number=\"42\" />", xml);
        }

        [TestMethod]
        public void SerializeXmlElementViaHelper()
        {
            var ob = new DemoPayload { Text = "Some Text", Number = 42 };

            var xml = XmlFragmentHelper.EncodeXmlFragment(ob);

            Assert.AreEqual("<DemoPayload Text=\"Some Text\" Number=\"42\" />", xml);
        }

        [TestMethod]
        public void DeserializeManyElements()
        {
            var xml = "<DemoPayload Text=\"One\" Number=\"1\" /><DemoPayload Text=\"Two\" Number=\"2\" />";

            var obs = XmlFragmentHelper.DecodeXmlFragments<DemoPayload>(xml);

            Assert.IsNotNull(obs);
            Assert.AreEqual(2, obs.Length);
            Assert.AreEqual("One", obs[0].Text);
            Assert.AreEqual(1, obs[0].Number);
            Assert.AreEqual("Two", obs[1].Text);
            Assert.AreEqual(2, obs[1].Number);
        }

        [TestMethod]
        public void XmlSaveAndRestore()
        {
            var random = new Random(0);
            var saveds = new DemoPayload[3];
            for (var i = 0; i < saveds.Length; i++)
            {
                saveds[i] = new DemoPayload { Text = i.ToString(), Number = random.Next(100, 1000) };
            }

            var builder = new StringBuilder();

            for (var i = 0; i < saveds.Length; i++)
            {
                var xml = XmlFragmentHelper.EncodeXmlFragment(saveds[i]);
                builder.AppendLine(xml);
            }

            var fileContent = builder.ToString();
            var restoreds = XmlFragmentHelper.DecodeXmlFragments<DemoPayload>(fileContent);

            Assert.IsNotNull(restoreds);
            Assert.AreEqual(saveds.Length, restoreds.Length);
            for (var i = 0; i < saveds.Length; i++)
            {
                Assert.AreEqual(saveds[i].Text, restoreds[i].Text);
                Assert.AreEqual(saveds[i].Number, restoreds[i].Number);
            }
        }

        [TestMethod]
        public void WriteAndReadLogFile()
        {
            var tempPath = Path.GetTempPath();
            var targetRootDirectory = Path.Combine(tempPath, Guid.NewGuid().ToString());
            var targetPath = Path.Combine(targetRootDirectory, "Testing", Guid.NewGuid().ToString(), "TestLogFile.xml");

            try
            {
                {
                    var payloads = XmlFragmentHelper.ReadLog<DemoPayload>(targetPath);

                    Assert.AreEqual(0, payloads.Length);
                }

                {
                    XmlFragmentHelper.WriteLog(targetPath, new DemoPayload { Text = "First", Number = 101 });

                    var lines = File.ReadAllLines(targetPath);
                    Assert.AreEqual(1, lines.Length);
                    Assert.AreEqual("<DemoPayload Text=\"First\" Number=\"101\" />", lines[0]);
                }

                {
                    XmlFragmentHelper.WriteLog(targetPath, new DemoPayload { Text = "Second", Number = 102 });

                    var lines = File.ReadAllLines(targetPath);
                    Assert.AreEqual(2, lines.Length);
                    Assert.AreEqual("<DemoPayload Text=\"First\" Number=\"101\" />", lines[0]);
                    Assert.AreEqual("<DemoPayload Text=\"Second\" Number=\"102\" />", lines[1]);
                }

                {
                    var payloads = XmlFragmentHelper.ReadLog<DemoPayload>(targetPath);

                    Assert.AreEqual(2, payloads.Length);
                    Assert.AreEqual("First", payloads[0].Text);
                    Assert.AreEqual(101, payloads[0].Number);
                    Assert.AreEqual("Second", payloads[1].Text);
                    Assert.AreEqual(102, payloads[1].Number);
                }
            }
            finally
            {
                Directory.Delete(targetRootDirectory, true);
            }
        }
    }
}
