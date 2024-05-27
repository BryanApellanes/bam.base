using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam;

namespace Bam
{
    public interface IObjectDecoder
    {
        object Decode(byte[] encoding, Type type);
        object Decode(IObjectEncoding encoding);
        T Decode<T>(byte[] encoding);
        T Decode<T>(IObjectEncoding<T> encoding);
    }
}
