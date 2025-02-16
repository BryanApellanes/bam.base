namespace Bam.Configuration
{
    public enum ConfigurationSources
    {
        NotFound,
        DefaultValue,
        NetCoreConfiguration,
        DefaultConfiguration,
        ConfigAppSettings, // Config.Current.AppSettings
        BamEnvironmentVariable,
        ConfigurationService
    }
}
