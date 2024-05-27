using System;

namespace Bam
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