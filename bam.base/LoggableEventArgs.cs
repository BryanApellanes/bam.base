using System;

namespace Bam.Net.Logging
{
    public class LoggableEventArgs: EventArgs
    {
        public ILoggable Sender { get; set; }
        public string Message { get; set; }
        public VerbosityLevel VerbosityLevel { get; set; }
        public VerbosityAttribute VerbosityAttribute { get; set; }

        public static LoggableEventArgs ForLoggable(ILoggable loggable, VerbosityAttribute verbosityAttribute)
        {
            return new LoggableEventArgs()
            {
                Sender = loggable,
                Message = verbosityAttribute.GetMessage(loggable, EventArgs.Empty),
                VerbosityLevel = verbosityAttribute.Value,
                VerbosityAttribute = verbosityAttribute
            };
        }
    }
}