namespace Bam
{
    public interface IExceptionReporter
    {
        void ReportException(string message, Exception exception);
        void ReportException(Exception exception);
    }
}
