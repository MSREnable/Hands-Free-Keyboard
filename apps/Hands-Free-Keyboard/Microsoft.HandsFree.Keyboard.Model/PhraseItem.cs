namespace Microsoft.HandsFree.Keyboard.Model
{
    public class PhraseItem : PredictionItem
    {
        internal PhraseItem(KeyboardHost host, int index)
            : base(host, index)
        {
        }

        protected override void AcceptAction(object o)
        {
            Suggestion.Accepted(Index);
            host.AcceptPhrase(this);
        }
    }
}
