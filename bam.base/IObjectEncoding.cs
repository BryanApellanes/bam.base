using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Storage;

namespace Bam
{
    public interface IObjectEncoding : IRawData
    {
        Type Type { get; }
    }
}
