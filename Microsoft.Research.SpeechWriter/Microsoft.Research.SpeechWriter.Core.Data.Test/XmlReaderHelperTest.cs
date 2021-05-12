using NUnit.Framework;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Data.Test
{
    public class XmlReaderHelperTest
    {
        [Test]
        public void ValidateDataTest()
        {
            for (var i = 0; i < 2; i++)
            {
                var threw = false;
                try
                {
                    XmlReaderHelper.ValidateData(i == 0);
                }
                catch (InvalidDataException)
                {
                    threw = true;
                }

                Assert.AreEqual(i != 0, threw);
            }
        }
    }
}
