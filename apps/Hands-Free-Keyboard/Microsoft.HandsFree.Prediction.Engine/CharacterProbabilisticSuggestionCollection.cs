namespace Microsoft.HandsFree.Prediction.Engine
{
    using Microsoft.HandsFree.LanguageHelpers;
    using Microsoft.HandsFree.Prediction.Api;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    class CharacterProbabilisticSuggestionCollection : IPredictionSuggestionCollection
    {
        List<IPredictionSuggestion> suggestions = new List<IPredictionSuggestion>(26);

        internal CharacterProbabilisticSuggestionCollection()
        {
            while (suggestions.Count < suggestions.Capacity)
            {
                suggestions.Add(null);
            }
        }

        internal CharacterProbabilisticSuggestionCollection(string text, int selection)
            : this()
        {
            Debug.Assert(text != null);
            Debug.Assert(0 <= selection);
            Debug.Assert(selection <= text.Length);

            var wordLength = text.ReverseWordLength(selection);

            if (0 < wordLength && wordLength < ProbabilityTable.MAX_WORD_LEN)
            {
                for (var ch = 'a'; ch <= 'z'; ch++)
                {
                    var probability = ProbabilityTable.GetProbability(wordLength,
                        char.ToLower(text[selection - 1]), ch);
                    suggestions[ch - 'a'] = new CharacterSuggestion(ch, probability);
                }
            }
            else
            {
                for (var ch = 'a'; ch <= 'z'; ch++)
                {
                    var probability = ProbabilityTable.GetProbability(0, ProbabilityTable.WordBeginChar, ch);
                    suggestions[ch - 'a'] = new CharacterSuggestion(ch, probability);
                }
            }
        }

        public string[] Context { get { return null; } }

        public IEnumerator<IPredictionSuggestion> GetEnumerator()
        {
            return suggestions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return suggestions.GetEnumerator();
        }
    }
}
