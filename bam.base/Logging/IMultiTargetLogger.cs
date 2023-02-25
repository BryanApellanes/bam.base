/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.Logging
{
    public interface IMultiTargetLogger : ILogger
    {
        ILogger[] Loggers { get; }

        void AddLogger(ILogger logger);
        void CommitLogEvent(LogEvent logEvent);
        ILogger StartLoggingThread();
    }
}