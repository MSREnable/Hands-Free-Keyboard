namespace Microsoft.HandsFree.Keyboard.Model
{
    public class SuggestionItem : PredictionItem
    {
        internal SuggestionItem(KeyboardHost host, int index)
            : base(host, index)
        {
        }

        protected override void AcceptAction(object o)
        {
            Suggestion.Accepted(Index);
            host.MakeSuggestion(this);
        }
    }
}
