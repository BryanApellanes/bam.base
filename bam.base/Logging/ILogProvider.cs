namespace Bam.Logging
{
    public interface ILogProvider
    {
        LogReaderFactory LogReaderFactory { get; set; }
        ILogger GetLogger();
        ILogReader GetLogReader();
    }
}
