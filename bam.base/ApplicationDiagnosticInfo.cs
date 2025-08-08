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
            ProcessId = Process.GetCurrentProcess().Id;
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
        }

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
        
        string appName;
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

        public override string ToString()
        {
            return NamedMessageFormat.NamedFormat(this);
        }
    }
}
