using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class BackgroundThreadQueueEventArgs: EventArgs
    {
        public Exception Exception { get; set; }
    }
}
