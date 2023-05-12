using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.ServiceProxy
{
    public interface IServiceProxyResponse<T> : IServiceProxyResponse
    { 
        new T Data { get; set; }
    }
}
