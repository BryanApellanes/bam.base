/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Logging
{
    [Serializable]
    public class LogEntry
    {
		public string Source
		{
			get;
			set;
		} = null!;

		public string Message
		{
			get;
			set;
		} = null!;

		public string Computer
		{
			get;
			set;
		} = null!;

		public Severity Severity
		{
			get;
			set;
		}

		public string Category
		{
			get;
			set;
		} = null!;

		public int EventID
		{
			get;
			set;
		}

		public string User
		{
			get;
			set;
		} = null!;

		public DateTime Time
		{
			get;
			set;
		}

        public string MessageSignature { get; set; } = null!;

        public string[] MessageVariableValues { get; set; } = null!;

        public string StackTrace { get; set; } = null!;
    }
}
