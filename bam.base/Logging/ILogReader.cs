/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Logging
{
	public interface ILogReader
	{
		ILogger Logger { get; set; }
		List<LogEntry> GetLogEntries(DateTime from, DateTime to);
	}
}
