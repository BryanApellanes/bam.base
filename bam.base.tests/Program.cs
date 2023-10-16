using Bam.CommandLine;
using Bam.Console;

namespace Bam.Net.Application
{
    [Serializable]
    class Program
    {
        static void Main(string[] args)
        {
            BamContext.Main(args);
        }
    }
}
