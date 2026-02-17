namespace Bam.Logging
{
    public class LoggableEventArgs: EventArgs
    {
        public ILoggable Sender { get; set; } = null!;
        public string Message { get; set; } = null!;
        public VerbosityLevel VerbosityLevel { get; set; }
        public VerbosityAttribute VerbosityAttribute { get; set; } = null!;

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
