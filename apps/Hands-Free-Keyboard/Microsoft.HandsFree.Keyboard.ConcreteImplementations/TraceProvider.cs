using System.Diagnostics;
using System.Reflection;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    /// <summary>
    /// Concrete implmentation of ITraceProvider.
    /// </summary>
    public class TraceProvider
    {
        /// <summary>
        /// Underlying TraceSource object.
        /// </summary>
        public static readonly TraceSource TraceSource;

        static TraceProvider()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var assemblyName = entryAssembly.GetName();
            TraceSource = new TraceSource(Assembly.GetEntryAssembly().GetName().Name, SourceLevels.All);
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        TraceProvider()
        {
        }
    }
}
