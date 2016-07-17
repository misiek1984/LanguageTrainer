using System;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace MK.Logging
{
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public sealed class ConsoleTraceListener : CustomTraceListener
    {
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
                                       object data)
        {
            var logEntry = data as LogEntry;
            if (logEntry != null)
            {
                if (Formatter != null)
                {
                    WriteLine(Formatter.Format(logEntry));
                }
                else
                {
                    WriteLine(String.Format("Timestamp: {0} Message: {1}", logEntry.TimeStamp, logEntry.Message));
                }
            }
            else
            {
                WriteLine(data.ToString());
            }
        }

        public override void Write(string message)
        {
            Console.Write(message);
        }
        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
