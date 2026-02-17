/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Logging
{
    [Serializable]
    public class LogEvent
    {
        string source= null!;
        string category= null!;
        int eventid;
        string user= null!;
        DateTime timeOccurred;
        string message= null!;
        string computer= null!;
        LogEventType type;

        public string Source
        {
            get => this.source;
            set => this.source = value;
        }

        public string Message
        {
            get => this.message;
            set => this.message = value;
        }

        public string Computer
        {
            get => this.computer;
            set => this.computer = value;
        }

        public LogEventType Severity
        {
            get => type;
            set => type = value;
        }
        
        public string Category
        {
            get => this.category;
            set => this.category = value;
        }

        public int EventID
        {
            get => this.eventid;
            set => this.eventid = value;
        }

        public string User
        {
            get => this.user;
            set => this.user = value;
        }

        public DateTime Time
        {
            get => this.timeOccurred;
            set => this.timeOccurred = value;
        }

        public string MessageSignature { get; set; } = null!;

        public string?[] MessageVariableValues { get; set; } = null!;

        public string StackTrace { get; set; } = null!;
    }
}
