using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.HandsFree.Helpers.Telemetry
{
    public abstract class TelemetryMessage
    {
        public static TraceSource Telemetry = new TraceSource(nameof(Telemetry));

        static TelemetryMessage()
        {
            Telemetry.Listeners.Add(new TelemetryTraceListener<EventId>());
            Telemetry.Switch.Level = SourceLevels.Verbose;
        }
    }

    public abstract class TelemetryMessage<T> : TelemetryMessage
        where T : struct
    {
        private const int HRTimeout = -2146233079;

        private const int MaxResends = 3;
        private const string BaseUri = "https://enabletelemetry.msrenableservices.com/";

        private readonly static string _userIdHash = CreateUserIdHash();

        internal string TableName;
        public string PartitionKey { get { return _userIdHash; } }
        public string AppVersion;
        public string MessageType { get; private set; }

        private int resendCount;

        public int MessageId
        {
            set { MessageType = IdMapper.GetName(value); }
        }

        protected TelemetryMessage(string tableName)
        {
            // Cannot set Enum as a generic constraint due to a language restriction, so checking this here.
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException("Generic Type T must be an enum");
            }

            AppVersion = AppVersionStatic;
            TableName = tableName;
        }

        static string CreateUserIdHash()
        {
            string userIdHash;

            using (var _sha256 = new SHA256Managed())
            {
                // See http://stackoverflow.com/questions/21144694/how-can-i-encode-azure-storage-table-row-keys-and-partition-keys
                userIdHash = Convert.ToBase64String(_sha256.ComputeHash(Encoding.UTF8.GetBytes(Environment.UserName))).Replace('/', '_');
            }

            return userIdHash;
        }

        private async Task SendAsync()
        {
            try
            {
                var logEventJson = JsonConvert.SerializeObject(new[] { this });
                var logEventJsonBytes = new UTF8Encoding().GetBytes(logEventJson);

                var uri = BaseUri + AppName + AppVersionInfo + TableName;

                var httpWebRequest = WebRequest.CreateHttp(uri);
                httpWebRequest.KeepAlive = false;
                httpWebRequest.Timeout = 15000;
                httpWebRequest.UserAgent = "keyboard";
                httpWebRequest.Method = "PUT";
                httpWebRequest.ContentType = "application/json;charset=utf-8";
                httpWebRequest.ContentLength = logEventJsonBytes.Length;
                using (var stream = await httpWebRequest.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(logEventJsonBytes, 0, logEventJsonBytes.Length);
                    await stream.FlushAsync();
                }

                using (var webResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync())  // Doesn't send if you don't call GetResponse
                {
                    Debug.Write(webResponse.StatusCode);

                    if (webResponse.StatusCode != HttpStatusCode.OK)
                    {
                        if (resendCount++ < MaxResends)
                        {
                            Enqueue();
                        }
                    }
                }
            }
            catch (WebException we)
            {
                if (we.HResult == HRTimeout)
                {
                    if (resendCount++ < MaxResends)
                    {
                        Enqueue();
                    }
                    else
                    {
                        Debug.Write("Dropping TelemetryMessage, too many resends");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.ToString());
            }
        }

        private static readonly string AppName;
        private static readonly string AppVersionInfo;
        private static readonly string AppVersionStatic;

        // Queue up messages to be send on a dedicated background thread
        private static readonly Queue<TelemetryMessage<T>> LogMessages = new Queue<TelemetryMessage<T>>();

        private static readonly SemaphoreSlim semaphoreQueueEntries = new SemaphoreSlim(0);
        private static readonly SemaphoreSlim semaphoreQueueEmpty = new SemaphoreSlim(0);

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly Task TransmitTask;

        static TelemetryMessage()
        {
            AppName = Assembly.GetEntryAssembly().GetName().Name.Split('.').Last();

            AppVersionInfo = ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute))).InformationalVersion;
            // Capitalize the _AppVersionInfo
            AppVersionInfo = AppVersionInfo.First().ToString().ToUpperInvariant() + string.Join("", AppVersionInfo.Skip(1));

            AppVersionStatic = Assembly.GetEntryAssembly().GetName().Version.ToString();

            TransmitTask = Task.Run(SendLoopAsync);
        }

        public void Enqueue()
        {
            lock (LogMessages)
            {
                LogMessages.Enqueue(this);
            }
            semaphoreQueueEntries.Release();
        }

        private static async Task SendLoopAsync()
        {
            for (;;)
            {
                if (!await semaphoreQueueEntries.WaitAsync(100))
                {
                    semaphoreQueueEmpty.Release();
                    await semaphoreQueueEntries.WaitAsync();
                    await semaphoreQueueEmpty.WaitAsync();
                }

                TelemetryMessage<T> message;
                lock (LogMessages)
                {
                    message = LogMessages.Dequeue();
                }
                await message.SendAsync();
            }
        }

        internal static void Flush()
        {
            if (semaphoreQueueEmpty.Wait(2000))
            {
                semaphoreQueueEmpty.Release();
            }
        }
    }
}
