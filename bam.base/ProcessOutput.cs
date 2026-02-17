/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Diagnostics;
using System.Text;

namespace Bam.CommandLine
{
    public class ProcessOutput
    {
        public ProcessOutput()
        {
            this.ExitCode = -1;
            this.StandardError = string.Empty;
            this.StandardOutput = string.Empty;
        }

        public ProcessOutput(Process process, StringBuilder output, StringBuilder errorOutput)
        {
            Process = process;
            ActiveStandardOut = output;
            ActiveStandardError = errorOutput;
        }

        public ProcessOutput(string output, string errorOutput, int exitCode, bool timedOut)
        {
            this.TimedOut = timedOut;
            this.StandardError = errorOutput;
            this.StandardOutput = output;
            this.ExitCode = exitCode;
        }

        public Process Process { get; } = null!;
        public bool TimedOut { get; set; }
        public int ExitCode { get; set; }
        public StringBuilder ActiveStandardOut { get; set; } = null!;
        public StringBuilder ActiveStandardError { get; set; } = null!;

        string _standardOutput = null!;
        public string StandardOutput
        {
            get
            {
                if(ActiveStandardOut != null)
                {
                    _standardOutput = ActiveStandardOut.ToString();
                }
                return _standardOutput;
            }
            set => _standardOutput = value;
        }
        string _standardError = null!;
        public string StandardError
        {
            get
            {
                if(ActiveStandardError != null)
                {
                    _standardError = ActiveStandardError.ToString();
                }
                return _standardError;
            }
            set => _standardError = value;
        }
    }
}
