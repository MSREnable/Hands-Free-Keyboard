namespace Microsoft.HandsFree.Keyboard.Model
{
    public enum NarrationEventType
    {
        /// <summary>
        /// Resetting the state of the model.
        /// </summary>
        Reset,

        VocalGesture,

        AcceptSuggestion,

        SimpleTyping,

        Simple,

        WordCompletion,

        Utter,

        FixedUtter,

        GotSuggestion
    }
}
