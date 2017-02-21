using Microsoft.HandsFree.Prediction.Api;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.HandsFree.Prediction.Engine
{
    class LuceneWordSuggestionCollection : IPredictionSuggestionCollection
    {
        readonly IPredictionEnvironment environment;
        readonly int replacementStart;
        readonly int replacementLength;
        readonly IEnumerable<string> rawSuggestions;

        public LuceneWordSuggestionCollection(IPredictionEnvironment environment, string[] context, int replacementStart, int replacementLength, IEnumerable<string> rawSuggestions)
        {
            this.environment = environment;
            Context = context;
            this.replacementStart = replacementStart;
            this.replacementLength = replacementLength;
            this.rawSuggestions = rawSuggestions;
        }

        public string[] Context { get; private set; }

        internal static IPredictionSuggestionCollection Create(IPredictionEnvironment environment, IWordSuggester wordSuggester, string text, int selectionStart, int selectionLength, bool isAutoSpace)
        {
            var prefixLength = isAutoSpace ? 0 : text.ReverseWordLength(selectionStart);
            var wordStart = selectionStart - prefixLength;
            var prefix = text.Substring(wordStart, prefixLength);

            var punctuationLength = text.ReversePunctuationLength(wordStart);
            var punctuationStart = wordStart - punctuationLength;
            var punctuation = text.Substring(punctuationStart, punctuationLength);

            var previousWords = new List<string>();

            if (punctuationStart != 0 && (punctuationLength == 0 || !punctuation.IsSentenceEnding()))
            {
                do
                {
                    var wordLength = text.ReverseWordLength(punctuationStart);
                    Debug.Assert(wordLength != 0);
                    var nextWordStart = punctuationStart - wordLength;
                    var word = text.Substring(nextWordStart, wordLength);

                    punctuationLength = text.ReversePunctuationLength(nextWordStart);
                    punctuationStart = nextWordStart - punctuationLength;
                    punctuation = text.Substring(punctuationStart, punctuationLength);

                    previousWords.Add(word);
                }
                while (punctuationStart != 0 && !punctuation.IsSentenceEnding());
            }

            var previousWordsArray = previousWords.ToArray();

            var rawSuggestions = wordSuggester.GetSuggestions(previousWordsArray, prefix);

            // Count the number of lowercase and uppercase letters in the prefix we're working from.
            var lowerCount = 0;
            var upperCount = 0;
            for (var i = 0; i < prefix.Length && lowerCount < 1; i++)
            {
                var ch = prefix[i];
                if (char.IsLower(ch))
                {
                    lowerCount++;
                }
                else
                {
                    upperCount++;
                }
            }

            Debug.Assert(lowerCount + upperCount <= prefix.Length, "Sanity");
            Debug.Assert(lowerCount == 0 || lowerCount == 1, "We shuold stop when we find the first lowercase letter");

            if (upperCount == 1 || (upperCount != 0 && lowerCount != 0))
            {
                // Just one uppercase suggests we're producing a capitalised word.
                rawSuggestions = rawSuggestions.Select((s) => s.Substring(0, 1).ToUpperInvariant() + s.Substring(1));
            }
            else if (lowerCount == 0 && upperCount != 0)
            {
                Debug.Assert(2 <= upperCount, "Should have seen two or more letters to pick ALL CAPS");

                rawSuggestions = rawSuggestions.Select((s) => s.ToUpperInvariant());
            }

            var collection = new LuceneWordSuggestionCollection(environment, previousWordsArray, wordStart, selectionStart - wordStart + selectionLength, rawSuggestions);

            return collection;
        }

        public IEnumerator<IPredictionSuggestion> GetEnumerator()
        {
            foreach (var rawSuggestion in rawSuggestions)
            {
                var suggestion = new LuceneWordSuggestion(environment, rawSuggestion, replacementStart, replacementLength);

                yield return suggestion;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
