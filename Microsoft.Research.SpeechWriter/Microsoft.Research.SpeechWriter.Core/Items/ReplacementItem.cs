namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Tile rewriting item.
    /// </summary>
    public class ReplacementItem : Command<WordVocabularySource>
    {
        private readonly string _replacement;

        internal ReplacementItem(HeadWordItem predecessor, WordVocabularySource source, string replacement)
            : base(predecessor, source)
        {
            OldContent = predecessor.Content;
            _replacement = replacement;
        }

        /// <summary>
        /// The old content.
        /// </summary>
        public string OldContent { get; }

        /// <summary>
        /// The content.
        /// </summary>
        public override string Content => _replacement;

        internal override void Execute(WordVocabularySource source)
        {
            source.ReplaceLastItem(_replacement);
        }
    }
}
