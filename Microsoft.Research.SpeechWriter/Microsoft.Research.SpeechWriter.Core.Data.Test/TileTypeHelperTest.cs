using NUnit.Framework;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Data.Test
{
    [Parallelizable(ParallelScope.All)]
    public class TileTypeHelperTest
    {
        [Test]
        public void FromElementNameTest()
        {
            Assert.AreEqual(TileType.Normal, TileTypeHelper.FromElementName("T"));
            Assert.AreEqual(TileType.Prefix, TileTypeHelper.FromElementName("B"));
            Assert.AreEqual(TileType.Suffix, TileTypeHelper.FromElementName("A"));
            Assert.AreEqual(TileType.Infix, TileTypeHelper.FromElementName("J"));
            Assert.AreEqual(TileType.Command, TileTypeHelper.FromElementName("C"));
            Assert.Throws(typeof(InvalidDataException), () => TileTypeHelper.FromElementName("!"));
        }

        [Test]
        public void ToElementNameTest()
        {
            Assert.AreEqual("T", TileTypeHelper.ToElementName(TileType.Normal));
            Assert.AreEqual("B", TileTypeHelper.ToElementName(TileType.Prefix));
            Assert.AreEqual("A", TileTypeHelper.ToElementName(TileType.Suffix));
            Assert.AreEqual("J", TileTypeHelper.ToElementName(TileType.Infix));
            Assert.AreEqual("C", TileTypeHelper.ToElementName(TileType.Command));
        }
    }
}
