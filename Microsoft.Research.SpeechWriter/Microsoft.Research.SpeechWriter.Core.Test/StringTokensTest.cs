using NUnit.Framework;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    public class StringTokensTest
    {
        [Test]
        public void ConstructionTest()
        {
            var words = DefaultWriterEnvironment.Instance.GetOrderedSeedWords();
            var tokens = StringTokens.Create(words);
            Assert.IsNotNull(tokens);

            for (var index = tokens.TokenLimit - 1; tokens.TokenStart <= index; index--)
            {
                var str = tokens[index];

                var token = tokens[str];
                Assert.AreEqual(index, token);

                var roundTrip = tokens[token];
                Assert.AreEqual(str, roundTrip);
            }

            var oldTokenLimit = tokens.TokenLimit;
            var newToken = tokens.GetToken("new lowercase string");
            Assert.AreEqual(oldTokenLimit, newToken);
            Assert.AreEqual(oldTokenLimit + 1, tokens.TokenLimit);
        }
    }
}
