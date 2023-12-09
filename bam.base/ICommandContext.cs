using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface ICommandContext
    {
        string ContextName { get; }
        IDictionary<string, ICommand> Commands { get; }
    }
}
