using Microsoft.HandsFree.Prediction.Api;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.HandsFree.Prediction.Engine
{
    class SingleLetterSuggester : IWordSuggester
    {
        readonly Dictionary<char, string[]> _letterWordsDictionary = new Dictionary<char, string[]>();

        readonly string[] _initialSuggestions;

        static readonly string[] EmptyWords = new string[0];

        internal static readonly SingleLetterSuggester Instance = new SingleLetterSuggester();

        SingleLetterSuggester()
        {
            var buildDictionary = new Dictionary<char, List<string>>();

            foreach (var mixedCaseWord in WordSource.SeedWords)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(mixedCaseWord));

                var lowerCaseWord = mixedCaseWord.ToLowerInvariant();

                List<string> letterWords;
                if (!buildDictionary.TryGetValue(lowerCaseWord[0], out letterWords))
                {
                    letterWords = new List<string>();
                    buildDictionary.Add(lowerCaseWord[0], letterWords);
                }

                letterWords.Add(lowerCaseWord);
            }

            foreach (var pair in buildDictionary)
            {
                _letterWordsDictionary.Add(pair.Key, pair.Value.ToArray());
            }

            _initialSuggestions = new string[WordSource.SeedWords.Length / 26];
            for (var i = 0; i < _initialSuggestions.Length; i++)
            {
                _initialSuggestions[i] = WordSource.SeedWords[i].ToLowerInvariant();
            }
        }

        public IEnumerable<string> GetSuggestions(string[] previousWords, string currentWordPrefix)
        {
            string[] letterWords;

            switch (currentWordPrefix.Length)
            {
                case 0:
                    letterWords = _initialSuggestions;
                    break;

                case 1:
                    if (!_letterWordsDictionary.TryGetValue(char.ToLowerInvariant(currentWordPrefix[0]), out letterWords))
                    {
                        letterWords = EmptyWords;
                    }
                    break;

                default:
                    letterWords = EmptyWords;
                    break;
            }

            return letterWords;
        }
    }
}
