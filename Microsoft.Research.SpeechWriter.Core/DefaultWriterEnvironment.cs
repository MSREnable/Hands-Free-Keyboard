using Microsoft.Research.SpeechWriter.Core.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Default implementation of IWriterEnvironment.
    /// </summary>
    public class DefaultWriterEnvironment : IWriterEnvironment
    {
        private readonly Comparison<string> _compare = (string1, string2) =>
               CultureInfo.CurrentUICulture.CompareInfo.Compare(string1, string2, CompareOptions.StringSort);

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DefaultWriterEnvironment()
        {
        }

        /// <summary>
        /// Singleton.
        /// </summary>
        public static IWriterEnvironment Instance { get; } = new DefaultWriterEnvironment();

        int IComparer<string>.Compare(string string1, string string2) => _compare(string1, string2);

        IEnumerable<string> IWriterEnvironment.GetOrderedSeedWords()
        {
#if true
            using (var reader = new StringReader(Resources.WordCountList))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var delimiter = line.IndexOf('\t');
                    var str = line.Substring(0, delimiter).ToUpperInvariant();

                    var goodLimit = 0;
                    while (goodLimit < str.Length && IsValidChar(str[goodLimit]))
                    {
                        goodLimit++;
                    }

                    if (goodLimit == str.Length)
                    {
                        yield return str;
                    }
                }
            }

            bool IsValidChar(char ch)
            {
                return char.IsLetter(ch) || ch == '-' || ch == '\'' || ch == '/';
            }
#else
            return new string[0];
#endif
        }

        IEnumerable<IEnumerable<string>> IWriterEnvironment.GetSeedSentences()
        {
#if true
            using (var reader = new StringReader(Resources.SentencesSeed))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var words = line.Split(' ');

                    yield return words;
                }
            }
#else
        return new string[][] { };
#endif
        }

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="words">The words of the utterance.</param>
        void IWriterEnvironment.SaveUtterance(string[] words)
        {
        }

        /// <summary>
        /// Recall persisted utterances.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        IEnumerable<string[]> IWriterEnvironment.RecallUtterances()
        {
            return new string[0][];
        }
    }
}
