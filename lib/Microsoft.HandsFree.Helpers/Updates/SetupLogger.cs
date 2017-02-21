using Splat;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microsoft.HandsFree.Helpers.Updates
{
    class SetupLogger : ILogger
    {
        readonly string file;

        public LogLevel Level { get; set; }

        public SetupLogger(bool saveInTemp)
        {
            var dir = saveInTemp ?
                Path.GetTempPath() :
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            file = Path.Combine(dir, "SquirrelSetup.log");
            File.Delete(file);
        }

        void ILogger.Write(string message, LogLevel logLevel)
        {
            if (logLevel < Level)
            {
                return;
            }

            lock (this)
            {
                File.AppendAllLines(file, new[] { message }, Encoding.UTF8);
            }
        }
    }
}
