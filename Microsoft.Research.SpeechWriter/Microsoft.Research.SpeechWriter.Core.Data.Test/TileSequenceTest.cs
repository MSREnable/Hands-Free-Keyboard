using NUnit.Framework;

namespace Microsoft.Research.SpeechWriter.Core.Data.Test
{
    public class TileSequenceTest
    {
        private static void SequenceSanityCheck(TileSequence sequence)
        {
            var encoded = sequence.ToHybridEncoded();
            var sequenceRemade = TileSequence.FromEncoded(encoded);
            Assert.AreEqual(sequence, sequenceRemade);
        }

        private static void SequenceSanityCheck(params TileData[] tiles)
        {
            var sequence = TileSequence.FromData(tiles);
            SequenceSanityCheck(sequence);
        }

        private static void RawSanityCheck(string raw)
        {
            var sequence = TileSequence.FromRaw(raw);

            SequenceSanityCheck(sequence);

            var rawRemade = sequence.ToRaw();
            Assert.AreEqual(raw, rawRemade);
        }

        [Test]
        public void HelloTest()
        {
            RawSanityCheck("Hello");
        }

        [Test]
        public void HelloWorldTest()
        {
            RawSanityCheck("Hello World!");
        }

        [Test]
        public void EmptyTest()
        {
            RawSanityCheck(string.Empty);
        }

        [Test]
        public void SpaceTest()
        {
            RawSanityCheck(" ");
        }

        [Test]
        public void TwinSpaceTest()
        {
            RawSanityCheck("  ");
        }

        [Test]
        public void RawThrashTest()
        {
            RawSanityCheck("Hello -Hello----");
            ThrashTestHelper.Run(8, s => RawSanityCheck(string.Join(string.Empty, s)), "-", "--", " ", "  ", "Hello");
        }

        [Test]
        public void HelloWorldSequenceCheck()
        {
            SequenceSanityCheck(new TileData("Hello"), new TileData("World"));
        }

        [Test]
        public void SpacingThrashTest()
        {
            ThrashTestHelper.Run(8, v => SequenceSanityCheck(v),
                new TileData("X", false, false),
                new TileData("X", true, false),
                new TileData("X", false, true),
                new TileData("X", true, true));
            ThrashTestHelper.Run(8, v => SequenceSanityCheck(v),
                new TileData("-", false, false),
                new TileData("-", true, false),
                new TileData("-", false, true),
                new TileData("-", true, true));

            ThrashTestHelper.Run(4, v => SequenceSanityCheck(v),
                new TileData("X", false, false),
                new TileData("X", true, false),
                new TileData("X", false, true),
                new TileData("X", true, true),
                new TileData("-", false, false),
                new TileData("-", true, false),
                new TileData("-", false, true),
                new TileData("-", true, true));
        }

        private static void CheckRoundTrip(string expected)
        {
            var collection = TileSequence.FromEncoded(expected);
            var actual = collection.ToHybridEncoded();
            Assert.AreEqual(expected, actual);
        }


        private static void CheckRoundTrip(string expected, string literal)
        {
            var collection = TileSequence.FromEncoded(literal);
            var actual = collection.ToHybridEncoded();
            Assert.AreEqual(expected, actual);

            CheckRoundTrip(actual);
        }

        [Test]
        public void GetDefaultEncodedFromRawTest()
        {
            var helloWorldEncoded = TileSequence.RawToDefaultSimpleEncoded("Hello World");
            Assert.AreEqual("Hello World", helloWorldEncoded);
            CheckRoundTrip(helloWorldEncoded);

            var elementEncoded = TileSequence.RawToDefaultSimpleEncoded("<Element/>");
            Assert.AreEqual("&lt;Element/&gt;", elementEncoded);
            //CheckRoundTrip(elementEncoded);

            var jackAndJillEncoded = TileSequence.RawToDefaultSimpleEncoded("Jack & Jill");
            Assert.AreEqual("Jack &amp; Jill", jackAndJillEncoded);
            //CheckRoundTrip(jackAndJillEncoded);
        }

        [Test]
        public void HelloWorldTrip() => CheckRoundTrip("Hello World", "<T>Hello</T><T>World</T>");

        [Test]
        public void HelloWorldTogetherTrip() => CheckRoundTrip("<B>Hello</B><A>World</A>");

        [Test]
        public void SimpleConstructionTest()
        {
            var sequence = TileSequence.FromData();
            Assert.IsNotNull(sequence);
        }

        [Test]
        public void ComplexishConstructionTest()
        {
            var sequence = TileSequence.FromData(new TileData("hello"), new TileData("world"));
            Assert.IsNotNull(sequence);
        }

        [Test]
        public void GoodAndBadXmlTest()
        {
            var texts = new[] { "<T>Hello</T><T>World</T>", "<T>Hello", "<!-- Comment -->" };
            foreach (var text in texts)
            {
                var good = true;
                try
                {
                    _ = TileSequence.FromEncoded(text);
                }
                catch
                {
                    good = false;
                }

                Assert.AreEqual(text == texts[0], good);
            }
        }

        [Test]
        public void EqualsTest()
        {
            var a = new TileData("A");
            var b = new TileData("B");

            var sequences = new[]
            {
                TileSequence.FromData(),
                TileSequence.FromData(a, a),
                TileSequence.FromData(a, b),
                TileSequence.FromData(b, a)
            };

            for (var i = 0; i < sequences.Length; i++)
            {
                for (var j = 0; j < sequences.Length; j++)
                {
                    var lhs = sequences[i];
                    var rhs = sequences[j];

                    var equals = lhs.Equals(rhs);
                    var lhsHash = lhs.GetHashCode();
                    var rhsHash = rhs.GetHashCode();

                    Assert.AreEqual(i == j, equals);
                    Assert.IsTrue(!equals || lhsHash == rhsHash);
                }
            }

            Assert.AreNotEqual(sequences[0], "Ethel the Aardvark");
        }
    }
}
