namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Interface providing raw narration hints.
    /// </summary>
    interface INarrator
    {
        /// <summary>
        /// Accept a narration event.
        /// </summary>
        /// <param name="e">The narration event arguments.</param>
        void OnNarrationEvent(NarrationEventArgs e);
    }
}
