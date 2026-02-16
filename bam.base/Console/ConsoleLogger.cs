/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Text;
using Bam.Logging;

namespace Bam.Console
{
    // TODO: break this up into ConsoleLogger and DetailConsoleLogger
    /// <summary>
    /// A logger implementation that writes color-coded log events to the console, with optional timestamps and diagnostic details.
    /// </summary>
    public class ConsoleLogger : Logger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class with colors, details, and timestamps enabled by default.
        /// </summary>
        public ConsoleLogger()
            : base()
        {
            AddDetails = true;
            UseColors = true;
            ShowTime = true;
        }

        /// <summary>
        /// Gets or sets whether console output colors are applied based on log event severity.
        /// </summary>
        public bool UseColors { get; set; }

        /// <summary>
        /// Gets or sets whether diagnostic detail information is included in log output.
        /// </summary>
        public bool AddDetails { get; set; }

        /// <summary>
        /// If true the Local time will prefix the output
        /// </summary>
        public bool ShowTime { get; set; }

        protected override StringBuilder HandleDetails(LogEvent ev)
        {
            if (AddDetails)
            {
                return base.HandleDetails(ev);
            }
            else
            {
                return new StringBuilder(ev.Message);
            }
        }

        /// <summary>
        /// Writes the specified log event to the console, applying color coding based on severity and optionally prefixing with a timestamp.
        /// </summary>
        /// <param name="logEvent">The log event to write to the console.</param>
        public override void CommitLogEvent(LogEvent logEvent)
        {
            if (UseColors)
            {
                switch (logEvent.Severity)
                {
                    case LogEventType.None:
                        System.Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case LogEventType.Information:
                        System.Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case LogEventType.Warning:
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogEventType.Error:
                        System.Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case LogEventType.Fatal:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
            }
            StringBuilder time = GetTimeString(logEvent);
            System.Console.WriteLine($"{time.ToString()}{logEvent.Message}");
            System.Console.ResetColor();
        }

        private StringBuilder GetTimeString(LogEvent logEvent)
        {
            StringBuilder time = new StringBuilder();
            if (ShowTime)
            {
                DateTime local = GetLocalTime(logEvent.Time);
                time.Append($"[Time({local.ToString()} ms {local.Millisecond})]");
            }

            return time;
        }

        private static DateTime GetLocalTime(DateTime input)
        {
            DateTime local = input;
            if (input.Kind == DateTimeKind.Utc)
            {
                local = input.ToLocalTime();
            }
            return local;
        }
    }
}
