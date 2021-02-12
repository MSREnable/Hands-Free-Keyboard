namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// A word in the suggestion list.
    /// </summary>
    public class SuggestedWordItem : WordItem
    {
        internal SuggestedWordItem(WordVocabularySource source, string word)
            : base(source, word)
        {
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.AddSuggestedWord(Word);
        }
    }
}
