namespace Bam.CommandLine
{
    public class ProcessOutputEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProcessOutput ProcessOutput { get; set; }
    }
}
