using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public static class ProcessExtensions
    {        public static ProcessStartInfo GetStartInfo(this Process process, params string[] processArgs)
        {
            ProcessStartInfo processStartInfo = CreateStartInfo(false);
            processStartInfo.FileName = new FileInfo(process.MainModule.FileName).FullName;
            processStartInfo.Arguments = string.Join(" ", processArgs);
            return processStartInfo;
        }

        internal static ProcessStartInfo CreateStartInfo()
        {
            return CreateStartInfo(false);
        }

        internal static ProcessStartInfo CreateStartInfo(bool promptForAdmin)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            if (promptForAdmin)
            {
                startInfo.Verb = "runas";
            }

            return startInfo;
        }
    }
}
