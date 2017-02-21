using System;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Provider of general context of running instance.
    /// </summary>
    public interface IInstanceIdentityProvider
    {
        /// <summary>
        /// Identifying name for current user.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Identifying name for current application.
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// Unique identifier for current session.
        /// </summary>
        Guid SessionGuid { get; }
    }
}
