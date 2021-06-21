using Microsoft.Research.SpeechWriter.Core.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

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

        int IComparer<string>.Compare(string string1, string string2)
        {
            var result = _compare(string1, string2);

            if (result == 0)
            {
                result = string.CompareOrdinal(string1, string2);
            }

            return result;
        }

        string IWriterEnvironment.Language => "en-US";

        /// <summary>
        /// Create reader for seed dictionary.
        /// </summary>
        /// <returns></returns>
        protected virtual StringReader CreateOrderedSeedWordsReader()
        {
            var reader = new StringReader(Resources.WordCountList);
            return reader;
        }

        IEnumerable<string> IWriterEnvironment.GetOrderedSeedWords()
        {
            var delimiters = "\t \r\n".ToCharArray();
            using (var reader = CreateOrderedSeedWordsReader())
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var delimiter = line.IndexOfAny(delimiters);
                    if (delimiter == -1)
                    {
                        delimiter = line.Length;
                    }
                    var str = line.Substring(0, delimiter);

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

            foreach (var symbol in new[] 
            { 
                ".\0A",
                ",\0A",
                "!\0A", 
                "?\0A",
                ":\0A",
                ";\0A",
                "\"\0B",
                "\"\0A",
                "'\0B",
                "'\0A",
                "'\0J",
                "-", 
                "#\0B", 
                "$\0B", 
                "%\0A", 
                "&", 
                "(\0B", 
                ")\0A", 
                "*", 
                "+", 
                "/\0J", 
                "<", 
                "=", 
                ">", 
                "@\0J", 
                "£\0A" })
            {
                yield return symbol;
            }

            bool IsValidChar(char ch)
            {
                return char.IsLetter(ch) || ch == '-' || ch == '\'' || ch == '/';
            }
        }

        /// <summary>
        /// Get the current time.
        /// </summary>
        /// <returns>The local time.</returns>
        DateTimeOffset IWriterEnvironment.GetTimestamp()
        {
            var timestamp = DateTimeOffset.Now;
            return timestamp;
        }

        /// <summary>
        /// Save a trace ine.
        /// </summary>
        /// <param name="trace"></param>
        /// <returns>The line to trace.</returns>
        Task IWriterEnvironment.SaveTraceAsync(string trace)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Persist an utterance.
        /// </summary>
        /// <param name="utterance">The utterance.</param>
        Task IWriterEnvironment.SaveUtteranceAsync(string utterance)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Recall persisted utterances.
        /// </summary>
        /// <returns>The collection of utterances.</returns>
        Task<TextReader> IWriterEnvironment.RecallUtterancesAsync()
        {
            var value = Task.FromResult(TextReader.Null);
            return value;
        }
    }
}
