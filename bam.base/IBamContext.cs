using Bam.Console;
using Bam.Net;
using Bam.Net.Configuration;
using Bam.Net.CoreServices;
using Bam.Net.Logging;

namespace Bam.Shell.Console
{
    public interface IBamContext
    {
        IApplicationNameProvider ApplicationNameProvider { get; }
        IConfigurationProvider ConfigurationProvider { get; }
        ILogger Logger { get; }

        ServiceRegistry ServiceRegistry { get; }

        void Configure(Action<ServiceRegistry> configure);
    }
}