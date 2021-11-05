using NUnit.Framework;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Data.Test
{
    [Parallelizable(ParallelScope.All)]
    public class XmlHelperTest
    {
        [Test]
        public void ValidateDataTest()
        {
            for (var i = 0; i < 2; i++)
            {
                var threw = false;
                try
                {
                    XmlHelper.ValidateData(i == 0);
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
