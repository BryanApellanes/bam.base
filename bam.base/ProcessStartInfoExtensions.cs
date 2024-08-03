using Bam.Console;
using Bam.CommandLine;
using System.Diagnostics;
using System.Text;

namespace Bam
{
    public static class ProcessStartInfoExtensions
    {

        /// <summary>
        /// Start a new process for the specified startInfo.  This 
        /// operation blocks if a timeout greater than 0 is specified
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ProcessOutput Run(this ProcessStartInfo startInfo, int timeout = 600000)
        {
            return Run(startInfo, new StringBuilder(), new StringBuilder(), timeout);
        }

        /// <summary>
        /// Run the specified command in a separate process capturing the output
        /// and error streams if any.
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ProcessOutput Run(this ProcessStartInfo startInfo, StringBuilder output, StringBuilder? error = null, int timeout = 600000)
        {
            output = output ?? new StringBuilder();
            error = error ?? new StringBuilder();
            ProcessOutputCollector receiver = new ProcessOutputCollector(output, error);
            return Run(startInfo, receiver, timeout);
        }

        /// <summary>
        /// Run the specified command in a separate process capturing the output and error streams if any.
        /// This method blocks until the process completes or the timeout elapses.  The default timeout is
        /// 600000.
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="dataHandler"></param>
        /// <param name="errorHandler"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ProcessOutput Run(this ProcessStartInfo startInfo, Action<string> dataHandler, Action<string>? errorHandler = null, int timeout = 600000)
        {
            return Run(startInfo, new ProcessOutputCollector(dataHandler, errorHandler), timeout);
        }

        /// <summary>
        /// Run the specified command in a separate process capturing the output
        /// and error streams if any
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="output"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ProcessOutput Run(this ProcessStartInfo startInfo, ProcessOutputCollector output, int? timeout = null)
        {
            return Run(startInfo, null, output, timeout);
        }
        /// <summary>
        /// Run the specified command in a separate process capturing the output
        /// and error streams if any. This method will block if a timeout is specified, it will
        /// not block if timeout is null.
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="onExit"></param>
        /// <param name="output"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ProcessOutput Run(this ProcessStartInfo startInfo, EventHandler onExit, ProcessOutputCollector? output = null, int? timeout = null)
        {
            int exitCode = -1;
            bool timedOut = false;
            output = output ?? new ProcessOutputCollector();

            Process process = new Process()
            {
                EnableRaisingEvents = true
            };
            if (onExit != null)
            {
                process.Exited += (o, a) => onExit(o, new ProcessExitEventArgs { EventArgs = a, ProcessOutput = new ProcessOutput(process, output.StandardOutput, output.StandardError) });
            }
            process.StartInfo = startInfo;
            AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
            AutoResetEvent errorWaitHandle = new AutoResetEvent(false);
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    outputWaitHandle.Set();
                }
                else
                {
                    output.DataHandler(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    errorWaitHandle.Set();
                }
                else
                {
                    output.ErrorHandler(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (timeout != null)
            {
                WaitForExit(output, timeout, ref exitCode, ref timedOut, process, outputWaitHandle, errorWaitHandle);
                return new ProcessOutput(output.StandardOutput.ToString(), output.StandardError.ToString(), exitCode, timedOut);
            }
            else
            {
                process.Exited += (o, a) =>
                {
                    Process p = (Process)o;
                    output.ExitCode = p.ExitCode;
                    p.Dispose();
                };
            }

            return new ProcessOutput(process, output.StandardOutput, output.StandardError);
        }

        private static void WaitForExit(ProcessOutputCollector output, int? timeout, ref int exitCode, ref bool timedOut, Process process, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle)
        {
            if (process.WaitForExit(timeout.Value) &&
                outputWaitHandle.WaitOne(timeout.Value) &&
                errorWaitHandle.WaitOne(timeout.Value))
            {
                exitCode = process.ExitCode;
                output.ExitCode = exitCode;
                process.Dispose();
            }
            else
            {
                output.StandardError.AppendLine();
                output.StandardError.AppendLine("Timeout elapsed prior to process completion");
                timedOut = true;
            }
        }

        private static ProcessStartInfo CreateStartInfo(bool promptForAdmin)
        {
            return ProcessExtensions.CreateStartInfo(promptForAdmin);
/*            ProcessStartInfo startInfo = new ProcessStartInfo
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

            return startInfo;*/
        }
    }
}
