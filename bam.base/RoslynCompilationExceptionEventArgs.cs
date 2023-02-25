using System;

namespace Bam.Net
{
    public class RoslynCompilationExceptionEventArgs : EventArgs
    {
        public RoslynCompilationExceptionEventArgs(RoslynCompilationException roslynCompilationException)
        {
            Exception = roslynCompilationException;
        }

        public RoslynCompilationException Exception { get; set; }
    }
}