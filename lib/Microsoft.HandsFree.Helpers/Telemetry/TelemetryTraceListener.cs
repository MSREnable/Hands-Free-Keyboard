using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.HandsFree.Helpers.Telemetry
{
    public class TelemetryTraceListener<T> : TraceListener where T : struct
    {
        private readonly Guid _sessionId = Guid.NewGuid();

        public TelemetryTraceListener()
        {
            // Cannot set Enum as a generic constraint due to a language restriction, so checking this here.
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException("Generic Type T must be an enum");
            }
        }

        /// <summary>
        /// Internal pure data class used for serialization purposes
        /// </summary>
        public class TraceMessage : TelemetryMessage<T>
        {
            public TraceMessage(string source) : base(source) { }

            public Guid SessionId;
            public string EventType;
            public string Message;
        }


        // None of these overrides make sense for a telemetry scenario because
        // they lack the requesite information by default to track the event
        // back to the source. As such, we actively block the usage of these
        // methods to prevent misuse.
        #region overrides throwing NotSupportedException
        public override void Write(object o)
        {
            throw new NotSupportedException();
        }

        public override void Write(object o, string category)
        {
            throw new NotSupportedException();
        }

        public override void Write(string message, string category)
        {
            throw new NotSupportedException();
        }

        public override void Write(string message)
        {
            throw new NotSupportedException();
        }

        public override void WriteLine(string message)
        {
            throw new NotSupportedException();
        }

        public override void WriteLine(object o)
        {
            throw new NotSupportedException();
        }

        public override void WriteLine(object o, string category)
        {
            throw new NotSupportedException();
        }

        public override void WriteLine(string message, string category)
        {
            throw new NotSupportedException();
        }

        public override void Fail(string message)
        {
            throw new NotSupportedException();
        }

        public override void Fail(string message, string detailMessage)
        {
            throw new NotSupportedException();
        }
        #endregion

        // These methods are not currently supported because they are not necessary. 
        // However, they have sufficient parameters to properly identify the event.
        #region NotSupported but might be reasonable to support
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            throw new NotSupportedException();
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            throw new NotSupportedException();
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region Supported trace methods
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            new TraceMessage(source)
            {
                SessionId = _sessionId,
                EventType = eventType.ToString(),
                MessageId = id,
                Message = message
            }.Enqueue();
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format,
            params object[] args)
        {
            TraceEvent(eventCache, source, eventType, id, string.Format(format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, string.Empty);
        }

        public override void Flush()
        {
            base.Flush();
            TelemetryMessage<T>.Flush();
        }
        #endregion
    }
}
