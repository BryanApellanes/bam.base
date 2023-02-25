using Bam.Net.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Bam.Net.Configuration
{
    public interface IConfigurationResolver
    {
        ConfigurationValue this[string key, string defaultValue = null, bool callConfigService = false] { get; }

        Dictionary<string, string> AppSettings { get; }
        Config Config { get; }
        IConfigurationProvider ConfigurationProvider { get; set; }
        NameValueCollection DefaultConfiguration { get; set; }
        ILogger Logger { get; set; }
        IConfiguration NetCoreConfiguration { get; set; }

        event EventHandler CalledConfigService;
        event EventHandler CallingConfigService;
        event EventHandler ConfigurationValueNotFound;
        event EventHandler RetrievedFromCache;
        event EventHandler RetrievedFromService;
        event EventHandler RetrievingFromService;
    }
}