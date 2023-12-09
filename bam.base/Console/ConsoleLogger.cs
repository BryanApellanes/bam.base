/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Net.Logging;
using System.Runtime.Serialization;

namespace Bam.Console
{
    // TODO: break this up into ConsoleLogger and DetailConsoleLogger
    public class ConsoleLogger : Logger
    {
        public ConsoleLogger()
            : base()
        {
            AddDetails = true;
            UseColors = true;
            ShowTime = true;
        }

        public bool UseColors { get; set; }
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
