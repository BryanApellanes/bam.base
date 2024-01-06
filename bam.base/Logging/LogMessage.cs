using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net.Logging
{
    public class LogMessage
    {
        public LogMessage() { }
        public LogMessage(string format, params string[] formatArgs)
        {
            Format = format;
            FormatArgs = formatArgs;
        }
        public string Format { get; set; }
        public string[] FormatArgs { get; set; }
        public Type SourceType { get; set; }
        public override string ToString()
        {
            return string.Format(Format, FormatArgs);
        }

        public virtual void Log(ILogger logger)
        {
            logger.AddEntry(Format, FormatArgs);
        }

        public virtual void Log(ILogger logger, LogEventType eventType)
        {
            string format = this.Format;
            if(this.SourceType != null)
            {
                format = $"{this.SourceType.FullName}::{this.Format}";
            }
            logger.AddEntry(format, eventType, FormatArgs);
        }
    }
}
