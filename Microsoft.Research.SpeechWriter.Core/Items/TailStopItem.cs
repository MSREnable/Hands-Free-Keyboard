namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Start item.
    /// </summary>
    public class TailStopItem : Command<WordVocabularySource>
    {
        private readonly string[] _words;

        internal TailStopItem(WordVocabularySource source, params string[] words)
            : base(source)
        {
            _words = words;
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.Commit(_words);
        }
    }
}
