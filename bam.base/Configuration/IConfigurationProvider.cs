using System.Collections.Generic;

namespace Bam.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfigurationProvider
    {
        Dictionary<string, string> GetApplicationConfiguration(string applicationName, string configurationName = "");
        void SetApplicationConfiguration(string applicationName, Dictionary<string, string> configuration, string configurationName);
    }
}