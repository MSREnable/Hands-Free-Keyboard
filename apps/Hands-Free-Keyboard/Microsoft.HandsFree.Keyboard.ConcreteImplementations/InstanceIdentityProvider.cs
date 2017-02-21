using Microsoft.HandsFree.Keyboard.Model;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    /// <summary>
    /// Concrete implementation of IInstanceIdentityProvider
    /// </summary>
    public class InstanceIdentityProvider : IInstanceIdentityProvider
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static readonly InstanceIdentityProvider Instance = new InstanceIdentityProvider();

        /// <summary>
        /// Private constructor.
        /// </summary>
        InstanceIdentityProvider()
        {
        }

        /// <summary>
        /// Identifying name for current user.
        /// </summary>
        public string UserId { get; } = string.Empty; //MsrSettings.GetSettings().UserId;

        /// <summary>
        /// Identifying name for current application.
        /// </summary>
        public string AppName { get; } = GetAppName();

        /// <summary>
        /// Unique identifier for current session.
        /// </summary>
        public Guid SessionGuid { get; } = Guid.NewGuid();

        static string GetAppName()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var name = entryAssembly.GetName();
            var informationalVersionAttribute = ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(entryAssembly, typeof(AssemblyInformationalVersionAttribute)));
            var informationalVersion = informationalVersionAttribute?.InformationalVersion;
            Debug.Assert(informationalVersion != null, "Need to define [assembly:AssemblyInformationalVersion(\"AA.BB.CC.DD\")]");

            var appName = $"{name.Name} {name.Version} {informationalVersion}";
            return appName;
        }
    }
}
