using Bam.Configuration;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Services;

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