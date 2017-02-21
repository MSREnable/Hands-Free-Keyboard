namespace Microsoft.HandsFree.Helpers.Updates
{
    enum UpdateTelemetryAction
    {
        /// <summary>
        /// Performed update poll, found no update.
        /// </summary>
        PolledNoUpdate,

        /// <summary>
        /// Performed update poll, found update waiting.
        /// </summary>
        PollFoundUpate,

        /// <summary>
        /// Creating shortcut for application.
        /// </summary>
        CreateShortcut,

        /// <summary>
        /// Removing shortcut for application.
        /// </summary>
        RemoveShortcut
    }
}