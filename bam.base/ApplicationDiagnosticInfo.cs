/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Bam.Net.Configuration;
using Bam.Net.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bam.Net
{
    /// <summary>
    /// Diagnostic information about the current
    /// application process and thread.
    /// </summary>
    [Serializable]
    public partial class ApplicationDiagnosticInfo
    {
        public const string DefaultMessageFormat = "Thread=#{ThreadHashCode}({ThreadId})~~App={ApplicationName}~~PID={ProcessId}~~Utc={UtcShortDate}::{UtcShortTime}~~{Message}";        
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

        public DateTime Local
        {
            get
            {
                return Utc.ToLocalTime();
            }
        }

        public string LocalShortTime
        {
            get
            {
                return this.Local.ToShortTimeString();
            }
        }

        public string LocalShortDate
        {
            get
            {
                return this.Local.ToShortDateString();
            }
        }

        public string UtcShortTime
        {
            get
            {
                return this.Utc.ToShortTimeString();
            }
        }

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
