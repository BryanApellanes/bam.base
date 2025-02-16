/*
	Copyright © Bryan Apellanes 2015  
*/

using System.Reflection;
using System.Text;
using Bam.DependencyInjection;
using Bam.Services;

namespace Bam.Data
{
    public interface IDaoProxyRegistration
    {
        Assembly Assembly { get; set; }
        string ContextName { get; set; }
        StringBuilder Ctors { get; }
        IDatabase Database { get; set; }
        StringBuilder MinCtors { get; }
        StringBuilder MinProxies { get; }
        StringBuilder Proxies { get; }
        DependencyProvider ServiceProvider { get; set; }
    }
}