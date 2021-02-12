namespace Microsoft.Research.RankWriter.Library.Items
{
    /// <summary>
    /// A word in the suggestion list.
    /// </summary>
    public class SuggestedWordSequenceItem : WordItem
    {
        private readonly string[] _words;

        internal SuggestedWordSequenceItem(WordVocabularySource source, string[] words)
            : base(source, words[words.Length - 1])
        {
            _words = words;
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.AddSuggestedSequence(_words);
        }
    }
}
