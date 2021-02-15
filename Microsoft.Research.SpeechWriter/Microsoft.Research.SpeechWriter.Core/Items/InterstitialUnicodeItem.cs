namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Unicode item.
    /// </summary>
    public class InterstitialUnicodeItem : Command<UnicodeVocabularySource>
    {
        internal InterstitialUnicodeItem(UnicodeVocabularySource source)
            : base(source)
        {
        }

        internal override void Execute(UnicodeVocabularySource source)
        {
            source.SetSuggestionsView();
        }
    }
}
