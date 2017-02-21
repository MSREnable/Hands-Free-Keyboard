using Microsoft.HandsFree.Keyboard.ConcreteImplementations;
using Microsoft.HandsFree.Keyboard.UserInterface;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Microsoft.HandsFree.Keyboard.Alternative
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "{0B85E77E-A87A-4209-9E90-61EE9337C4BF}";

        /// <summary>
        /// Entry point.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                application.InitializeComponent();

                ThemeManager.ThemeApplication();

                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
            else
            {
                MessageBox.Show($"{Assembly.GetEntryAssembly().GetName().Name} is already running.");
            }
        }

        #region ISingleInstanceApp Members

        bool ISingleInstanceApp.SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // …

            return true;
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public App()
        {
            //var listener = new FileLogTraceListener
            //{
            //    AutoFlush = true,
            //    BaseFileName = Assembly.GetEntryAssembly().GetName().Name,
            //    Location = LogFileLocation.Custom,
            //    CustomLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            //    LogFileCreationSchedule = LogFileCreationScheduleOption.Weekly,
            //    TraceOutputOptions = TraceOptions.DateTime
            //};
            //_trace.Listeners.Add(listener);

            DispatcherUnhandledException += (o, e) =>
            {
                var ex = e.Exception;

                // Grabbing the InnerException if its there to get closer to the source.
                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                //TelemetryUtility.Log(new ExceptionEntry()
                //{
                //    UserId = InstanceIdentityProvider.Instance.UserId,
                //    Message = ex.Message,
                //    StackTrace = ex.StackTrace,
                //    Application = Assembly.GetEntryAssembly().GetName().Name,
                //    Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                //    ApplicationCreation = File.GetLastWriteTime(Assembly.GetEntryAssembly().Location).ToString("u"),
                //});

                TraceProvider.TraceSource.TraceEvent(TraceEventType.Critical, 0, ex.ToString());
            };

            Exit += OnExit;

            TraceProvider.TraceSource.TraceEvent(TraceEventType.Information, 0, "Application Started");
        }

        private void OnExit(object o, object args)
        {
            TraceProvider.TraceSource.TraceEvent(TraceEventType.Information, 0, "Application Exited");
            TraceProvider.TraceSource.Flush();
        }
    }
}
