using System;
using System.IO;

namespace Bam
{
    public class ConfigChangedEventArgs: EventArgs
    {
        public FileInfo File { get; set; }
        public Config OldConfig { get; set; }
        public Config NewConfig { get; set; }
    }
}