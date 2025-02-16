/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Security.Principal;

namespace Bam.ServiceProxy
{
    public interface IHttpContext
    {
        IRequest Request { get; set; }
        IResponse Response { get; set; }
        IPrincipal User { get; set; }
    }
}
