/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Diagnostics;
using Bam.Logging;

namespace Bam
{
    /// <summary>
    /// Diagnostic information about the current
    /// application process and thread.
    /// </summary>
    [Serializable]
    public partial class ApplicationDiagnosticInfo
    {
        public const string DefaultMessageFormat = "Thread=#{ThreadHashCode}({ThreadId})~~App={ApplicationName}~~PID={ProcessId}~~Utc={UtcShortDate}::{UtcShortTime}~~Local={LocalShortDate}::{LocalShortTime}~~{Message}";        
        public const string UnknownApplication = "UNKNOWN-APPLICATION";
        public const string PublicOrganization = "PUBLIC-ORGANIZATION";

        public ApplicationDiagnosticInfo()
        {
            NamedMessageFormat = DefaultMessageFormat;
            Utc = DateTime.UtcNow;
            ThreadHashCode = Thread.CurrentThread.GetHashCode();
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            ProcessId = Environment.ProcessId;
        }

        public ApplicationDiagnosticInfo(LogEvent logEvent)
            : this()
        {
            this.Message = logEvent.Message;
        }

        public string NamedMessageFormat
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        } = null!;

        public int ProcessId
        {
            get;
            set;
        }

        public DateTime Utc
        {
            get;
            set;
        }

        public string UtcShortDate
        {
            get
            {
                return this.Utc.ToShortDateString();
            }
        }

        public DateTime Local => Utc.ToLocalTime();

        public string LocalShortTime => this.Local.ToShortTimeString();

        public string LocalShortDate => this.Local.ToShortDateString();

        public string UtcShortTime => this.Utc.ToShortTimeString();

        public int ThreadHashCode
        {
            get;
            set;
        }

        public int ThreadId
        {
            get;
            set;
        }
        
        string appName = null!;
        public string ApplicationName
        {
            get
            {
                if (string.IsNullOrEmpty(appName) || appName.Equals(UnknownApplication))
                {
                    appName = ApplicationNameProvider.Default.GetApplicationName().Or(UnknownApplication);
                }
                return appName;
            }
            set
            {
                appName = value;
            }
        }

        /// <summary>
        /// Formats <see cref="NamedMessageFormat"/>. The named tokens are filled in first and the message is inserted
        /// last, so token-like text inside the message (for example <c>{ThreadId}</c> in a logged request path) is
        /// written as it is and never substituted.
        /// </summary>
        public override string ToString()
        {
            const string messageToken = "{Message}";
            string[] parts = (NamedMessageFormat ?? string.Empty).Split(messageToken);
            // Read the properties once, then fill every part from the same values.
            Dictionary<string, string?> values = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (System.Reflection.PropertyInfo property in GetType().GetProperties())
            {
                if (property.Name != nameof(Message) && property.Name != nameof(NamedMessageFormat) && property.GetIndexParameters().Length == 0)
                {
                    values[property.Name] = property.GetValue(this)?.ToString();
                }
            }
            for (int index = 0; index < parts.Length; index++)
            {
                parts[index] = parts[index].NamedFormat(values);
            }
            return string.Join(Message ?? string.Empty, parts);
        }
    }
}
