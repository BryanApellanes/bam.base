using Bam.Console;
using Bam;
using Bam.Configuration;
using Bam.CoreServices;
using Bam.Logging;

namespace Bam
{
    public interface IBamContext
    {
        IApplicationNameProvider ApplicationNameProvider { get; }
        IConfigurationProvider ConfigurationProvider { get; }
        ILogger Logger { get; }

        ServiceRegistry ServiceRegistry { get; }
    }
}