using System.Reflection;
using Bam.Logging;

namespace Bam
{
    /// <summary>
    /// Provides application configuration backed by YAML files, with support for per-application settings,
    /// file change watching, and environment variable integration.
    /// </summary>
    public class Config: Loggable
    {
        static Config()
        {
            ApplicationNameProvider = ProcessApplicationNameProvider.Current;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class using the current process application name,
        /// and subscribes to file change notifications.
        /// </summary>
        public Config() : this(true)
        {
        }

        private Config(bool subscribeToChanges)
        {
            ApplicationName = ProcessApplicationNameProvider.Current.GetApplicationName();
            AppSettings = Read(out FileInfo file);
            File = file;

            if(subscribeToChanges)
            {
                ConfigChangeWatcher = File.OnChange((o, a) =>
                {
                    Config oldConfig = new Config(false)
                    {
                        AppSettings = AppSettings
                    };
                    AppSettings = Read();
                    Config newConfig = this;
                    ConfigChangedEventArgs args = new ConfigChangedEventArgs()
                    {
                        OldConfig = oldConfig,
                        NewConfig = newConfig
                    };
                    FireEvent(ConfigChanged, this, args);
                });
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class for the specified application name,
        /// and subscribes to file change notifications.
        /// </summary>
        /// <param name="applicationName">The application name used to locate the configuration file.</param>
        public Config(string applicationName) : this(applicationName, true)
        {
        }

        private Config(string applicationName, bool subscribeToChanges)
        {
            ApplicationName = applicationName;
            AppSettings = Read(applicationName, out FileInfo file);
            File = file;

            if (subscribeToChanges)
            {
                ConfigChangeWatcher = File.OnChange((o, a) =>
                {
                    Config oldConfig = new Config(applicationName, false)
                    {
                        AppSettings = AppSettings
                    };
                    AppSettings = Read(applicationName);
                    Config newConfig = this;
                    ConfigChangedEventArgs args = new ConfigChangedEventArgs()
                    {
                        OldConfig = oldConfig,
                        NewConfig = newConfig
                    };
                    FireEvent(ConfigChanged, this, args);
                });
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class for the specified application name and config file path,
        /// and subscribes to file change notifications.
        /// </summary>
        /// <param name="applicationName">The application name associated with this configuration.</param>
        /// <param name="configFilePath">The absolute path to the configuration file.</param>
        public Config(string applicationName, string configFilePath) : this(applicationName, configFilePath, true)
        {
        }

        private Config(string applicationName, string configFilePath, bool subscribeToChanges)
        {
            ApplicationName = applicationName;
            File = new FileInfo(configFilePath);
            AppSettings = Read(File);

            if (subscribeToChanges)
            {
                ConfigChangeWatcher = File.OnChange((o, a) =>
                {
                    Config oldConfig = new Config(applicationName, configFilePath, false)
                    {
                        AppSettings = AppSettings
                    };
                    AppSettings = Read(File);
                    Config newConfig = this;
                    ConfigChangedEventArgs args = new ConfigChangedEventArgs()
                    {
                        OldConfig = oldConfig,
                        NewConfig = newConfig
                    };
                    FireEvent(ConfigChanged, this, args);
                });
            }
        }

        /// <summary>
        /// Gets the application name this configuration is associated with.
        /// </summary>
        public string ApplicationName { get; private set; }
        
        static Config _current;
        static readonly object _currentLock = new object();
        
        /// <summary>
        /// Config for the current process; may be overwritten.
        /// </summary>
        public static Config Current
        {
            get { return _currentLock.DoubleCheckLock(ref _current, () => new Config()); }
            set => _current = value;
        }
        
        /// <summary>
        /// Creates a new <see cref="Config"/> for the current application, optionally using the specified config file path.
        /// </summary>
        /// <param name="configFilePath">Optional path to a specific configuration file. If null, the default path is used.</param>
        /// <returns>A new <see cref="Config"/> instance.</returns>
        public static Config Create(string? configFilePath = null)
        {
            return For(ApplicationNameProvider.GetApplicationName(), configFilePath);
        }

        /// <summary>
        /// Creates a new <see cref="Config"/> for the specified application name and optional config file path.
        /// </summary>
        /// <param name="applicationName">The application name to load configuration for.</param>
        /// <param name="configFilePath">Optional path to a specific configuration file.</param>
        /// <returns>A new <see cref="Config"/> instance.</returns>
        public static Config For(string applicationName, string? configFilePath = null)
        {
            return new Config(applicationName, configFilePath);
        }

        /// <summary>
        /// Creates a new <see cref="Config"/> for the specified application name.
        /// </summary>
        /// <param name="applicationName">The application name to load configuration for.</param>
        /// <returns>A new <see cref="Config"/> instance.</returns>
        public static Config For(string applicationName)
        {
            return new Config(applicationName);
        }
        
        /// <summary>
        /// Gets or sets the file system watcher that monitors the configuration file for changes.
        /// </summary>
        protected FileSystemWatcher ConfigChangeWatcher { get; set; }

        /// <summary>
        /// Raised when the underlying configuration file changes on disk.
        /// </summary>
        public event EventHandler ConfigChanged;

        /// <summary>
        /// Gets or sets the <see cref="FileInfo"/> representing the configuration file on disk.
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        /// Gets the workspace for the current application.
        /// </summary>
        public Workspace Workspace => Workspace.Current;

        /// <summary>
        /// Gets or sets the dictionary of application settings (key-value pairs) loaded from the configuration file.
        /// </summary>
        public Dictionary<string, string> AppSettings { get; set; }
        
        /// <summary>
        /// Gets or sets a configuration value by key. On get, if the key is not found and a default is provided,
        /// the default is persisted to the config file. On set, the value is stored and the config file is saved.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">The default value to use if the key is not found.</param>
        /// <returns>The configuration value, or the default value if the key was not found.</returns>
        public string? this[string key, string? defaultValue = null]
        {
            get
            {
                if (AppSettings.TryGetValue(key, out var item))
                {
                    return item;
                }

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    AppSettings.Add(key, defaultValue);
                    Write(AppSettings);
                }

                return defaultValue;
            }
            set
            {
                if (AppSettings.ContainsKey(key))
                {
                    AppSettings[key] = string.IsNullOrEmpty(value) ? defaultValue: value;
                }
                else
                {
                    AppSettings.Add(key, string.IsNullOrEmpty(value) ? defaultValue: value);
                }
                Save();
            }
        }

        static IApplicationNameProvider _applicationNameProvider;
        /// <summary>
        /// Gets or sets the application name provider used to determine config file paths.
        /// Defaults to <see cref="ProcessApplicationNameProvider.Current"/>.
        /// </summary>
        public static IApplicationNameProvider ApplicationNameProvider
        {
            get => _applicationNameProvider = _applicationNameProvider ?? ProcessApplicationNameProvider.Current;

            set => _applicationNameProvider = value;
        }

        /// <summary>
        /// Reads application settings from the BAM profile configuration file.
        /// </summary>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file is empty.</returns>
        public static Dictionary<string, string> ReadFromProfile()
        {
            return ReadFromProfile(out FileInfo ignore);
        }

        /// <summary>
        /// Reads application settings from the BAM profile configuration file and outputs the file reference.
        /// </summary>
        /// <param name="configFile">When this method returns, contains the <see cref="FileInfo"/> of the profile config file.</param>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file is empty.</returns>
        public static Dictionary<string, string> ReadFromProfile(out FileInfo configFile)
        {
            configFile = GetBamProfileConfigFile();
            return configFile.FromYamlFile<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        }
        
        /// <summary>
        /// Reads application settings from the BAM home configuration file for the current application.
        /// </summary>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file is empty.</returns>
        public static Dictionary<string, string> Read()
        {
            return Read(out FileInfo ignore);
        }

        /// <summary>
        /// Reads application settings from the BAM home configuration file and outputs the file reference.
        /// </summary>
        /// <param name="configFile">When this method returns, contains the <see cref="FileInfo"/> of the config file.</param>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file is empty.</returns>
        public static Dictionary<string, string> Read(out FileInfo configFile)
        {
            configFile = GetBamHomeConfigFile();
            return configFile.FromYamlFile<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Reads application settings from the BAM home configuration file for the specified application name.
        /// </summary>
        /// <param name="applicationName">The application name used to locate the configuration file.</param>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file is empty.</returns>
        public static Dictionary<string, string> Read(string applicationName)
        {
            return Read(applicationName, out FileInfo ignore);
        }

        /// <summary>
        /// Reads application settings from the BAM home configuration file for the specified application name and outputs the file reference.
        /// </summary>
        /// <param name="applicationName">The application name used to locate the configuration file.</param>
        /// <param name="configFile">When this method returns, contains the <see cref="FileInfo"/> of the config file.</param>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file is empty.</returns>
        public static Dictionary<string, string> Read(string applicationName, out FileInfo configFile)
        {
            configFile = GetBamHomeConfigFile(applicationName);
            return configFile.FromYamlFile<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Reads application settings from the specified configuration file, creating it if it does not exist.
        /// </summary>
        /// <param name="configFile">The configuration file to read from.</param>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file is empty.</returns>
        public static Dictionary<string, string> Read(FileInfo configFile)
        {
            configFile = EnsureFile(configFile.FullName);
            return configFile.FromYamlFile<Dictionary<string, string>>() ?? new Dictionary<string, string>();            
        }

        /// <summary>
        /// Saves the current <see cref="AppSettings"/> to the configuration file as YAML.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="File"/> has not been set.</exception>
        public void Save()
        {
            if (File == null)
            {
                throw new InvalidOperationException("File not set");
            }
            AppSettings.ToYaml().SafeWriteToFile(File.FullName, true);
        }
        
        /// <summary>
        /// Writes the specified settings to the BAM home configuration file, merging with existing values.
        /// Optionally sets BAM environment variables for each key.
        /// </summary>
        /// <param name="appSettings">The settings to write.</param>
        /// <param name="setBamEnvironmentVariables">If true, sets a BAM environment variable for each key-value pair.</param>
        public static void Write(Dictionary<string, string> appSettings, bool setBamEnvironmentVariables = true)
        {
            if (setBamEnvironmentVariables)
            {
                foreach (string key in appSettings.Keys)
                {
                    BamEnvironmentVariables.SetBamVariable(key, appSettings[key]);
                }
            }
            FileInfo configFile = GetBamHomeConfigFile();
            if (configFile.Exists)
            {
                Dictionary<string, string> existing = configFile.FullName.FromYamlFile<Dictionary<string, string>>() ?? new Dictionary<string, string>();
                foreach (string key in existing.Keys)
                {
                    appSettings.AddMissing(key, existing[key]);
                }
            }
            appSettings.ToYaml().SafeWriteToFile(configFile.FullName, true);
        }

        /// <summary>
        /// Sets <see cref="Current"/> to a new <see cref="Config"/> loaded for the specified application name provider.
        /// </summary>
        /// <param name="applicationNameProvider">The application name provider used to locate the configuration file. Must not be null.</param>
        /// <returns>The newly created and set <see cref="Config"/> instance.</returns>
        public static Config Set(IApplicationNameProvider applicationNameProvider)
        {
            Args.ThrowIfNull(applicationNameProvider, "applicationNameProvider");
            FileInfo configFile = GetBamHomeConfigFile(applicationNameProvider);
            Config config = new Config
            {
                AppSettings = configFile.FromYamlFile<Dictionary<string, string>>() ?? new Dictionary<string, string>()
            };
            Current = config;
            return config;
        }

        /// <summary>
        /// Load an instance of T from bam configs, creating the config file if necessary.
        /// </summary>
        /// <param name="applicationNameProvider"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>(IApplicationNameProvider? applicationNameProvider = null) where T : class, new()
        {
            DirectoryInfo processDir = GetDirectory(applicationNameProvider);
            string fileName = $"{typeof(T).Namespace}.{typeof(T).Name}.config.yaml";
            FileInfo file = new FileInfo(Path.Combine(processDir.FullName, fileName));
            if (!file.Exists)
            {
                (new T()).ToYamlFile(file);
            }
            return file.FromYamlFile<T>();
        }
        
        /// <summary>
        /// Reads type-specific application settings for type T from a YAML file in the configuration directory.
        /// </summary>
        /// <typeparam name="T">The type whose name determines the settings file name.</typeparam>
        /// <param name="applicationNameProvider">Optional application name provider to determine the config directory. Defaults to the current process.</param>
        /// <returns>A dictionary of key-value settings, or an empty dictionary if the file does not exist or is empty.</returns>
        public static Dictionary<string, string> AppSettingsFor<T>(IApplicationNameProvider? applicationNameProvider = null)
        {
            string fileName = $"{typeof(T).Namespace}.{typeof(Type).Name}.appsettings.yaml";
            FileInfo file = new FileInfo(Path.Combine(GetDirectory(applicationNameProvider).FullName, fileName));
            if (file.Exists)
            {
                return file.FromYamlFile<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            }
            return new Dictionary<string, string>();
        }
        
        /// <summary>
        /// Gets the configuration directory for the specified application name provider
        /// </summary>
        /// <param name="applicationNameProvider"></param>
        /// <returns></returns>
        public static DirectoryInfo GetDirectory(IApplicationNameProvider? applicationNameProvider = null)
        {
            DirectoryInfo configDir = GetBamHomeConfigFile().Directory;
			applicationNameProvider = applicationNameProvider ?? ProcessApplicationNameProvider.Current;
            string typeConfigsFolderName = applicationNameProvider.GetApplicationName();
            if (string.IsNullOrEmpty(typeConfigsFolderName))
            {
                typeConfigsFolderName = ApplicationDiagnosticInfo.UnknownApplication;//Bam.CoreServices.ApplicationRegistration.Data.Application.Unknown.Name;
            }
            
            return new DirectoryInfo(Path.Combine(configDir.FullName, typeConfigsFolderName));
        }
        
        /// <summary>
        /// Gets the BAM home configuration file for the specified application name provider, creating the file if it does not exist.
        /// </summary>
        /// <param name="applicationNameProvider">Optional application name provider. Defaults to <see cref="ProcessApplicationNameProvider.Current"/>.</param>
        /// <returns>The <see cref="FileInfo"/> for the configuration file.</returns>
        public static FileInfo GetBamHomeConfigFile(IApplicationNameProvider? applicationNameProvider = null)
        {
            applicationNameProvider = applicationNameProvider ?? ProcessApplicationNameProvider.Current;
            Log.Trace("Config using applicationNameProvider of type ({0})", applicationNameProvider?.GetType().Name);
            string providedAppName = applicationNameProvider.GetApplicationName();
            return GetBamHomeConfigFile(providedAppName);
        }
        
        /// <summary>
        /// Get the config file for the specified application from the `.bam` directory of the process
        /// owner's profile.
        /// </summary>
        /// <param name="applicationNameProvider"></param>
        /// <returns></returns>
        public static FileInfo GetBamProfileConfigFile(IApplicationNameProvider? applicationNameProvider = null)
        {
			applicationNameProvider = applicationNameProvider ?? ProcessApplicationNameProvider.Current;
            Log.Trace("Config using applicationNameProvider of type ({0})", applicationNameProvider?.GetType().Name);
            string providedAppName = applicationNameProvider.GetApplicationName();
            return GetBamProfileConfigFile(providedAppName);
        }

        /// <summary>
        /// Gets the BAM profile configuration file for the specified application name, creating the file if it does not exist.
        /// </summary>
        /// <param name="appName">The application name used to construct the file path.</param>
        /// <returns>The <see cref="FileInfo"/> for the profile configuration file.</returns>
        public static FileInfo GetBamProfileConfigFile(string appName)
        {
            string assemblyFile = Assembly.GetEntryAssembly().GetFileInfo().FullName;
            string assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
            string path = !appName.StartsWith("UNKNOWN")
                ? Path.Combine(BamProfile.ConfigPath, appName, $"{appName}.appsettings.yaml")
                : Path.Combine(BamProfile.ConfigPath, assemblyName, $"{assemblyName}.appsettings.yaml");
            Log.Trace("config file path = {0}", path);
            FileInfo configFile = EnsureFile(path);

            return configFile;
        }
        
        /// <summary>
        /// Gets the BAM home configuration file for the specified application name, creating the file if it does not exist.
        /// </summary>
        /// <param name="appName">The application name used to construct the file path.</param>
        /// <returns>The <see cref="FileInfo"/> for the home configuration file.</returns>
        public static FileInfo GetBamHomeConfigFile(string appName)
        {
            string assemblyFile = Assembly.GetEntryAssembly().GetFileInfo().FullName;
            string assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
            string path = !appName.StartsWith("UNKNOWN")
                ? Path.Combine(BamHome.ConfigPath, appName, $"{appName}.appsettings.yaml")
                : Path.Combine(BamHome.ConfigPath, assemblyName, $"{assemblyName}.appsettings.yaml");
            Log.Trace("config file path = {0}", path);
            FileInfo configFile = EnsureFile(path);

            return configFile;
        }
        
        /// <summary>
        /// Gets the name of the entry assembly without extension.
        /// </summary>
        /// <returns></returns>
        public static string GetHostServiceName()
        {
            string assemblyFile = Assembly.GetEntryAssembly().GetFileInfo().FullName;
            return Path.GetFileNameWithoutExtension(assemblyFile);
        }
        
        protected static FileInfo EnsureFile(string path)
        {
            FileInfo configFile = new FileInfo(path);
            if (!configFile.Exists)
            {
                if (!configFile.Directory.Exists)
                {
                    configFile.Directory.Create();
                }

                System.IO.File.Create(configFile.FullName).Dispose();
            }

            return configFile;
        }
    }
}