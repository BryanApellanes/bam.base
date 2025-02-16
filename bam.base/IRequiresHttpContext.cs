/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.ServiceProxy
{
    public interface IRequiresHttpContext: ICloneable
    {
        IHttpContext HttpContext { get; set; }
    }
}
