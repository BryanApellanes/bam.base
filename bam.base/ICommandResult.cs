using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface ICommandResult
    {
        string Message { get; }
        bool Success { get; }
        object Result { get; }
        string CommandName { get; }
        string[] Arguments { get; }
    }
}
