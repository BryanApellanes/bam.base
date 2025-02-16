/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Logging
{
    /// <summary>
    /// The same values as LogEventType.
    /// Both exist for clarity in specific
    /// contexts.
    /// </summary>
    public enum VerbosityLevel : int
    {
        None = 0,
        Fatal = 1,
        Error = 2,
        Warning = 3,
        Information = 4,
        Custom = 5
    }
}
