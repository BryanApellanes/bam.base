/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Logging
{
    public interface IMultiTargetLogger : ILogger
    {
        ILogger[] Loggers { get; }

        void AddLogger(ILogger logger);
    }
}