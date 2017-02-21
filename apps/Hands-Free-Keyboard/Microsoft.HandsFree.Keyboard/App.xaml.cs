using Microsoft.HandsFree.Helpers.Updates;
using Microsoft.HandsFree.Keyboard.ConcreteImplementations;
using Microsoft.HandsFree.Keyboard.UserInterface;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.HandsFree.Keyboard
{
    public partial class App : ISingleInstanceApp
    {
        private const string Unique = "{88845501-12F4-4254-9866-E4AFA3825316}";

        [STAThread]
        public static void Main()
        {
            UpdateHost.Start(true, TimeSpan.FromMinutes(1));

            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                application.InitializeComponent();

                ThemeManager.ThemeApplication();

                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // …

            return true;
        }

        #endregion

        public App()
        {
            DispatcherUnhandledException += (o, e) =>
            {
                var ex = e.Exception;

                // Grabbing the InnerException if its there to get closer to the source.
                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                TraceProvider.TraceSource.TraceEvent(TraceEventType.Critical, 0, ex.ToString());
            };

            TraceProvider.TraceSource.TraceEvent(TraceEventType.Information, 0, "Application Started");
        }
    }
}
