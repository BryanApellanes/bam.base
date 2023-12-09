using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface ICommandBroker : ICommandContext
    {
        // TODO: implement this as a mechanism that wraps a class and redricts method invocations to other processes or netowrk listeners

        IDictionary<string, ICommandContext> Contexts { get; }

        ICommandResult Execute(string commandName, string[] arguments);
    }
}
