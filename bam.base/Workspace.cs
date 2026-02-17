using Bam.Configuration;

namespace Bam
{
    /// <summary>
    /// Provides an interface to a specific place in the filesystem.
    /// </summary>
    public class Workspace
    {
        /// <summary>
        /// Gets or sets the application name provider associated with this workspace.
        /// </summary>
        public IApplicationNameProvider ApplicationNameProvider { get; set; } = null!;

        /// <summary>
        /// Gets or sets the root directory of this workspace.
        /// </summary>
        public DirectoryInfo Root { get; set; } = null!;

        /// <summary>
        /// Creates a directory at the path formed by joining the specified segments relative to the workspace root.
        /// Returns the existing directory if it already exists.
        /// </summary>
        /// <param name="pathSegments">Path segments relative to the workspace root.</param>
        /// <returns>The created or existing <see cref="DirectoryInfo"/>.</returns>
        public DirectoryInfo CreateDirectory(params string[] pathSegments)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path(pathSegments));
            if (!directoryInfo.Exists)
            {
                return System.IO.Directory.CreateDirectory(directoryInfo.FullName);
            }

            return directoryInfo;
        }
        
        /// <summary>
        /// Create a file for the specified path relative to the workspace.
        /// </summary>
        /// <param name="pathSegments"></param>
        /// <returns></returns>
        public FileInfo CreateFile(params string[] pathSegments)
        {
            FileInfo file = new FileInfo(Path(pathSegments));
            if (!file.Exists)
            {
                System.IO.File.Create(file.FullName).Dispose();
            }

            return file;
        }

        /// <summary>
        /// Get a directory for the specified path relative to the workspace
        /// </summary>
        /// <param name="pathSegments"></param>
        /// <returns></returns>
        public DirectoryInfo Directory(params string[] pathSegments)
        {
            return new DirectoryInfo(Path(pathSegments));
        }
        
        /// <summary>
        /// Get a file for the specified path segments relative to workspace.
        /// </summary>
        /// <param name="pathSegments"></param>
        /// <returns></returns>
        public FileInfo File(params string[] pathSegments)
        {
            return new FileInfo(Path(pathSegments));
        }
        
        /// <summary>
        /// Get a path for the specified path segments relative to workspace.
        /// </summary>
        /// <param name="pathSegments"></param>
        /// <returns></returns>
        public string Path(params string[] pathSegments)
        {
            List<string> fileSegments = new List<string> {Root.FullName};
            fileSegments.AddRange(pathSegments);
            return System.IO.Path.Combine(fileSegments.ToArray());
        }

        /// <summary>
        /// Output to the console and write that output to the Workspace console log.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLine(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
            string message = $"{string.Format(format, args)}\r\n";
            FileInfo file = new FileInfo(Path("Console"));
            if (file.Exists && file.Length >= 1048576)
            {
                file = file.GetNextFile();
            }
            message.SafeAppendToFile(file.FullName);
        }
        
        /// <summary>
        /// Save the specified object instance as a yaml file
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public FileInfo Save(object instance)
        {
            Args.ThrowIfNull(instance, "instance");
            Type type = instance.GetType();
            FileInfo file = CreateFile($"{type.Namespace}", $"{type.Name}.yaml");
            instance.ToYamlFile(file);
            return file;
        }

        /// <summary>
        /// Loads an instance of type T from a YAML file in the workspace, named by the type's namespace and name.
        /// </summary>
        /// <typeparam name="T">The type to deserialize from the YAML file.</typeparam>
        /// <returns>The deserialized instance of T.</returns>
        public T Load<T>()
        {
            Type type = typeof(T);
            FileInfo file = CreateFile($"{type.Namespace}", $"{type.Name}.yaml");
            return file.FromYamlFile<T>();
        }

        //TextFileLogger _logger;
/*        public ILogger CreateLogger<T>() where T : TextFileLogger, new()
        {
            if (_logger == null)
            {
                _logger = new T {Folder = Root};
            }

            return _logger;
        }*/
        
        static Workspace _current= null!;
        static readonly object _currentLock = new object();
        
        /// <summary>
        /// Workspace for the current application, see also ForProcess().
        /// </summary>
        public static Workspace Current
        {
            get { return _currentLock.DoubleCheckLock(ref _current, () => ForApplication()); }
        }
        
        /// <summary>
        /// Creates a workspace scoped to type T, located under the application workspace in a process-mode-specific subdirectory.
        /// </summary>
        /// <typeparam name="T">The type to scope the workspace to.</typeparam>
        /// <param name="applicationNameProvider">Optional application name provider. Defaults to the current process.</param>
        /// <returns>A <see cref="Workspace"/> rooted at the type-specific directory.</returns>
        public static Workspace ForType<T>(IApplicationNameProvider? applicationNameProvider = null)
        {
            return ForType(typeof(T), applicationNameProvider);
        }

        /// <summary>
        /// Creates a workspace scoped to the specified type, located under the application workspace in a process-mode-specific subdirectory.
        /// </summary>
        /// <param name="type">The type to scope the workspace to.</param>
        /// <param name="applicationNameProvider">Optional application name provider. Defaults to the current process.</param>
        /// <returns>A <see cref="Workspace"/> rooted at the type-specific directory.</returns>
        public static Workspace ForType(Type type, IApplicationNameProvider? applicationNameProvider = null)
        {
            applicationNameProvider = applicationNameProvider ?? ProcessApplicationNameProvider.Current;
            Workspace applicationWorkspace = ForApplication(applicationNameProvider);
            string directoryPath =
                System.IO.Path.Combine(applicationWorkspace.Root.FullName, ProcessMode.Current.Mode.ToString(), $"{type.Namespace}.{type.Name}");
            return new Workspace() {ApplicationNameProvider = applicationNameProvider, Root = new DirectoryInfo(directoryPath)};
        }

        /// <summary>
        /// Creates a workspace for the current process using <see cref="ProcessApplicationNameProvider.Current"/>.
        /// </summary>
        /// <returns>A <see cref="Workspace"/> rooted at the current process application directory.</returns>
        public static Workspace ForProcess()
        {
            return ForApplication(ProcessApplicationNameProvider.Current);
        }

        /// <summary>
        /// Creates a workspace for the specified application name provider under the BAM home apps directory.
        /// </summary>
        /// <param name="applicationNameProvider">Optional application name provider. Defaults to the current process.</param>
        /// <returns>A <see cref="Workspace"/> rooted at the application directory.</returns>
        public static Workspace ForApplication(IApplicationNameProvider? applicationNameProvider = null)
        {
            applicationNameProvider = applicationNameProvider ?? ProcessApplicationNameProvider.Current;
            //Log.Trace(typeof(Workspace), "Workspace using applicationNameProvider of type ({0})", applicationNameProvider?.GetType().Name);
            string directoryPath = System.IO.Path.Combine(BamHome.AppsPath, applicationNameProvider.GetApplicationName());
            return new Workspace() {ApplicationNameProvider = applicationNameProvider, Root = new DirectoryInfo(directoryPath)};
        }

        /// <summary>
        /// Creates a workspace for the specified application name under the BAM home apps directory.
        /// </summary>
        /// <param name="applicationName">The application name to create the workspace for.</param>
        /// <returns>A <see cref="Workspace"/> rooted at the application directory.</returns>
        public static Workspace ForApplication(string applicationName)
        {
            return new Workspace()
            {
                ApplicationNameProvider = new StaticApplicationNameProvider(applicationName),
                Root = new DirectoryInfo(System.IO.Path.Combine(BamHome.AppsPath, applicationName))
            };
        }
    }
}
