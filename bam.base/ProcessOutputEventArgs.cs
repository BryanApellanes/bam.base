namespace Bam.CommandLine
{
    public class ProcessOutputEventArgs : EventArgs
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ProcessOutput ProcessOutput { get; set; } = null!;
    }
}
