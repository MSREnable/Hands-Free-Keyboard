using Microsoft.Research.SpeechWriter.Core.Database;
using NUnit.Framework;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Test.Database
{
    public class CompressedIntWriterTest
    {
        [Test]
        public void ReadWriteIntTest()
        {
            var stream = new MemoryStream();
            var writer = new CompressedIntWriter(stream);

            for (var expected = -42; expected < 300; expected++)
            {
                writer.Write(expected);
            }
            writer.Write(int.MaxValue);
            writer.Write(int.MinValue);

            stream.Position = 0;
            var reader = new CompressedIntReader(stream);

            for (var expected = -42; expected < 300; expected++)
            {
                var actual = reader.ReadInt();
                Assert.AreEqual(expected, actual);
            }

            var actualMaxInt = reader.ReadInt();
            Assert.AreEqual(int.MaxValue, actualMaxInt);

            var actualMinInt = reader.ReadInt();
            Assert.AreEqual(int.MinValue, actualMinInt);
        }

        [Test]
        public void EdgeCaseTest()
        {
            var stream = new MemoryStream();
            var writer = new CompressedIntWriter(stream);

            writer.Write(256);

            stream.Position = 0;
            var reader = new CompressedIntReader(stream);

            var actual256 = reader.ReadInt();
            Assert.AreEqual(256, actual256);
        }

        [Test]
        public void StringTest()
        {
            var stream = new MemoryStream();
            var writer = new CompressedIntWriter(stream);

            writer.Write("Hello");
            writer.Write("World");

            stream.Position = 0;
            var reader = new CompressedIntReader(stream);

            var actualHello = reader.ReadString();
            Assert.AreEqual("Hello", actualHello);

            var actualWorld = reader.ReadString();
            Assert.AreEqual("World", actualWorld);
        }
    }
}
