using Bam.Configuration;

namespace Bam
{
    /// <summary>
    /// Represents the mode that the current process is
    /// in.  Intended primarily to determine where to
    /// save and retrieve data from but can be used
    /// to pivot other configuration values as well.
    /// </summary>
    public class ProcessMode
    {
        /// <summary>
        /// Gets or sets the <see cref="ProcessModes"/> value representing this process mode.
        /// </summary>
        public ProcessModes Mode { get; set; }

        /// <summary>
        /// Gets a <see cref="ProcessMode"/> by reading the "ProcessMode" key from the default application configuration,
        /// defaulting to <see cref="ProcessModes.Dev"/>.
        /// </summary>
        public static ProcessMode FromConfig
        {
            get
            {
                string fromConfig = DefaultConfiguration.GetAppSetting("ProcessMode", nameof(ProcessModes.Dev));
                return FromString(fromConfig);
            }
        }

        /// <summary>
        /// Gets a <see cref="ProcessMode"/> by reading the "ProcessMode" key from the BAM configuration,
        /// defaulting to <see cref="ProcessModes.Dev"/>.
        /// </summary>
        public static ProcessMode FromBamConfig
        {
            get
            {
                string fromConfig = Config.Current["ProcessMode", nameof(ProcessModes.Dev)];
                return FromString(fromConfig);
            }
        }

        /// <summary>
        /// Gets a <see cref="ProcessMode"/> from the BAM environment variable.
        /// </summary>
        public static ProcessMode FromEnvironment
        {
            get { return new ProcessMode { Mode = BamEnvironmentVariables.ProcessMode() }; }
        }

        static ProcessMode _current;
        /// <summary>
        /// Gets or sets the current process mode. First checks for a <c>--ProcessMode:&lt;value&gt;</c> command-line argument,
        /// then falls back to <see cref="FromBamConfig"/>.
        /// </summary>
        public static ProcessMode Current
        {
            get
            {
                if (_current == null)
                {
                    string processModeArg = Environment.GetCommandLineArgs()
                        .FirstOrDefault(a => a.StartsWith("--ProcessMode"));
                    if (!string.IsNullOrEmpty(processModeArg))
                    {
                        string[] split = processModeArg.DelimitSplit(":");
                        if (split.Length == 2)
                        {
                            _current = FromString(split[1]);
                        }
                    }
                }

                return _current ??= FromBamConfig;
            }
            set => _current = value;
        }

        /// <summary>
        /// Gets a <see cref="ProcessMode"/> representing the Dev environment.
        /// </summary>
        public static ProcessMode Dev { get { return new ProcessMode { Mode = ProcessModes.Dev }; } }

        /// <summary>
        /// Gets a <see cref="ProcessMode"/> representing the Test environment.
        /// </summary>
        public static ProcessMode Test { get { return new ProcessMode { Mode = ProcessModes.Test }; } }

        /// <summary>
        /// Gets a <see cref="ProcessMode"/> representing the Prod environment.
        /// </summary>
        public static ProcessMode Prod { get { return new ProcessMode { Mode = ProcessModes.Prod }; } }

        /// <summary>
        /// Parses a <see cref="ProcessMode"/> from its string representation.
        /// </summary>
        /// <param name="value">The string to parse (e.g., "Dev", "Test", "Prod").</param>
        /// <returns>A <see cref="ProcessMode"/> with the parsed <see cref="ProcessModes"/> value.</returns>
        public static ProcessMode FromString(string value)
        {
            return new ProcessMode { Mode = (ProcessModes)Enum.Parse(typeof(ProcessModes), value) };
        }

        /// <summary>
        /// Creates a <see cref="ProcessMode"/> from a <see cref="ProcessModes"/> enum value.
        /// </summary>
        /// <param name="enumVal">The process mode enum value.</param>
        /// <returns>A <see cref="ProcessMode"/> wrapping the specified enum value.</returns>
        public static ProcessMode FromEnum(ProcessModes enumVal)
        {
            return new ProcessMode { Mode = enumVal };
        }

        public override int GetHashCode()
        {
            return Mode.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is ProcessMode mode)
            {
                return mode.Mode.Equals(Mode);
            }
            return false;
        }

        public override string ToString()
        {
            return Mode.ToString();
        }
    }
}
