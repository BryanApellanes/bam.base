using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public interface IObjectEncoding
    {
        Type Type { get; }
        string Name { get; }
        byte[] Bytes { get; }
    }
}
