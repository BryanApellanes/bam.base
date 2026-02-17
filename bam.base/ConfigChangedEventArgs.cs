namespace Bam
{
    public class ConfigChangedEventArgs: EventArgs
    {
        public FileInfo File { get; set; } = null!;
        public Config OldConfig { get; set; } = null!;
        public Config NewConfig { get; set; } = null!;
    }
}
