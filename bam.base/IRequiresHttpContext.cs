/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.ServiceProxy
{
    public interface IRequiresHttpContext: ICloneable
    {
        IHttpContext HttpContext { get; set; }
    }
}
