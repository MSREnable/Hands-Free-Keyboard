using Microsoft.Research.SpeechWriter.Core;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Apps.Uwp
{
    internal class NonEnglishWriterEnvironment : DefaultWriterEnvironment, IWriterEnvironment
    {
        private readonly string _language;
        private readonly string _seedWords;

        internal NonEnglishWriterEnvironment(string language, string seedWords)
        {
            _language = language;
            _seedWords = seedWords;
        }

        string IWriterEnvironment.Language => _language;

        protected override StringReader CreateOrderedSeedWordsReader()
        {
            var reader = new StringReader(_seedWords);
            return reader;
        }
    }
}
