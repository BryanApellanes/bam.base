using System;

namespace Bam.Logging
{
    public class MessageEventArgs: EventArgs
    {
        public LogEventType LogEventType { get; set; }
        public LogMessage LogMessage { get; set; }
    }
}