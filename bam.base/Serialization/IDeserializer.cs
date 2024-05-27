using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public interface IDeserializer
    {

        object? Deserialize(byte[] data, Type type);
    }
}
