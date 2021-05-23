namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A word in the suggestion list for spelling where sorting semantics are slightly different..
    /// </summary>
    public class SuggestedSpellingWordItem : WordItem, ISuggestionItem
    {
        internal SuggestedSpellingWordItem(ITile predecessor, WordVocabularySource source, string word)
            : base(predecessor, source, word)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.AddSuggestedWord(Content);
        }

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            return null;
        }
    }
}
