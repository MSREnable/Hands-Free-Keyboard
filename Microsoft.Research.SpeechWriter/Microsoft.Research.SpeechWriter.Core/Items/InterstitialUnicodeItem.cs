namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Unicode item.
    /// </summary>
    public class InterstitialUnicodeItem : Command<UnicodeVocabularySource>
    {
        internal InterstitialUnicodeItem(ITile predecessor, UnicodeVocabularySource source)
            : base(predecessor, source)
        {
        }

        internal override void Execute(UnicodeVocabularySource source)
        {
            source.SetSuggestionsView();
        }
    }
}
