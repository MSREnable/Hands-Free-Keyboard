namespace Microsoft.HandsFree.Keyboard.Controls
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Helper for raising suggestions.
    /// </summary>
    public static class SuggestionsHelper
    {
        /// <summary>
        /// Set the new top suggestions.
        /// </summary>
        /// <param name="suggestions"></param>
        public static void SetSuggestions(IList<string> suggestions)
        {
            var handler = SuggestionsChanged;
            if (handler != null)
            {
                var e = new SuggestionsEventArgs(suggestions);
                handler(null, e);
            }
        }

        /// <summary>
        /// The top suggestion.
        /// </summary>
        public static EventHandler<SuggestionsEventArgs> SuggestionsChanged;
    }
}
