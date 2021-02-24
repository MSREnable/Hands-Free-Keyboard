﻿using Microsoft.Research.SpeechWriter.Core.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
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

        int IComparer<string>.Compare(string string1, string string2) => _compare(string1, string2);

        IEnumerable<string> IWriterEnvironment.GetOrderedSeedWords()
        {
#if true
            var delimiters = "\t \r\n".ToCharArray();
            using (var reader = new StringReader(Resources.WordCountList))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var delimiter = line.IndexOfAny(delimiters);
                    if (delimiter == -1)
                    {
                        delimiter = line.Length;
                    }
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
        IAsyncEnumerable<string[]> IWriterEnvironment.RecallUtterances()
        {
            return new EmptyAsyncEnumerable<string[]>();
        }

        private class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new EmptyAsyncEnumerator();
            }

            private class EmptyAsyncEnumerator : IAsyncEnumerator<T>
            {
                public T Current => default;

                public ValueTask DisposeAsync()
                {
                    return new ValueTask(Task.CompletedTask);
                }

                public ValueTask<bool> MoveNextAsync()
                {
                    return new ValueTask<bool>(Task.FromResult(false));
                }
            }
        }
    }
}
