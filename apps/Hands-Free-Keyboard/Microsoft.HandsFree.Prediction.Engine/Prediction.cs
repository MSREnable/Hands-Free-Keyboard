using Microsoft.HandsFree.Prediction.Api;
using System.Diagnostics;

namespace Microsoft.HandsFree.Prediction.Engine
{
    class Prediction : IPrediction
    {
        readonly IPredictionEnvironment environment;
        readonly IWordSuggester wordSuggester;
        readonly IPhraseSuggester phraseSuggester;
        readonly string text;
        readonly int selectionStart;
        readonly int selectionLength;
        readonly bool isAutoSpace;

        internal Prediction(IPredictionEnvironment environment, IWordSuggester wordSuggester, IPhraseSuggester phraseSuggester, string text, int selectionStart, int selectionLength, bool isAutoSpace)
        {
            Debug.Assert(text != null);

            this.environment = environment;
            this.wordSuggester = wordSuggester;
            this.phraseSuggester = phraseSuggester;
            this.text = text;
            this.selectionStart = selectionStart;
            this.selectionLength = selectionLength;
            this.isAutoSpace = isAutoSpace;
        }

        public IPredictionSuggestionCollection GetSuggestions(SuggestionType type)
        {
            IPredictionSuggestionCollection suggestions;

            switch (type)
            {
                case SuggestionType.Character:
                    suggestions = new CharacterProbabilisticSuggestionCollection(text, selectionStart);
                    break;

                case SuggestionType.Word:
                    suggestions = LuceneWordSuggestionCollection.Create(environment, wordSuggester, text, selectionStart, selectionLength, isAutoSpace);
                    break;

                default:
                    suggestions = EmptySuggestions.Instance;
                    break;
            }

            return suggestions;
        }

        internal IPredictionSuggestionCollection GetPhraseSuggestion(string[] prefix)
        {
            var suggestion = phraseSuggester.GetPhraseSuggestion(prefix);
            var collection = new LuceneWordSuggestionCollection(environment, null, 0, 0, suggestion);
            return collection;
        }
    }
}
