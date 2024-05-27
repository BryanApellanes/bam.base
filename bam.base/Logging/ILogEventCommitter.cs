namespace Bam.Logging
{
    public interface ILogEventCommitter
    {
        void CommitLogEvent(LogEvent logEvent);
    }
}