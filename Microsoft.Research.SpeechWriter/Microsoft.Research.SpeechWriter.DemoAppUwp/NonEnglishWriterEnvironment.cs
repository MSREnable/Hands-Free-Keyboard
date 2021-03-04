using Microsoft.Research.SpeechWriter.Core;
using System.IO;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    class NonEnglishWriterEnvironment : DefaultWriterEnvironment, IWriterEnvironment
    {
        private string _seedWords;

        internal NonEnglishWriterEnvironment(string seedWords)
        {
            _seedWords = seedWords;
        }

        protected override StringReader CreateOrderedSeedWordsReader()
        {
            var reader = new StringReader(_seedWords);
            return reader;
        }
    }
}
