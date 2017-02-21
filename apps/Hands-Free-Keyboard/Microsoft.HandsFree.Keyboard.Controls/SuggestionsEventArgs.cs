namespace Microsoft.HandsFree.Keyboard.Controls
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Suggestion event args.
    /// </summary>
    public class SuggestionsEventArgs : EventArgs
    {
        internal SuggestionsEventArgs(IList<string> suggestions)
        {
            Suggestions = suggestions;
        }

        /// <summary>
        /// The suggested word.
        /// </summary>
        public IList<string> Suggestions { get; private set; }
    }
}
