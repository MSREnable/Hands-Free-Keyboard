namespace Microsoft.Research.RankWriter.Library.Automation
{
    /// <summary>
    /// Names for the lists targetted by an ApplicationRobotAction.
    /// </summary>
    public enum ApplicationRobotActionTarget
    {
        /// <summary>
        /// An item from the Head list.
        /// </summary>
        Head,

        /// <summary>
        /// An item from teh Tail list.
        /// </summary>
        Tail,

        /// <summary>
        /// An item from the Interstitual list.
        /// </summary>
        Interstitial,

        /// <summary>
        /// An item from the SuggestionList.
        /// </summary>
        Suggestion
    }
}
