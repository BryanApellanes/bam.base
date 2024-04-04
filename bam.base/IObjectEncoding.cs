using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Storage;

namespace Bam.Net
{
    public interface IObjectEncoding : IRawData
    {
        Type Type { get; }
        byte[] Value { get; }
    }
}
