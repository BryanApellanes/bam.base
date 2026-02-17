using Bam.CommandLine;

namespace Bam.Console
{
    public class ProcessExitEventArgs : EventArgs
    {
        public EventArgs EventArgs { get; set; } = null!;
        public ProcessOutput ProcessOutput { get; set; } = null!;
    }
}
