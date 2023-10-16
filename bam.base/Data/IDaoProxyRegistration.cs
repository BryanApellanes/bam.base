/*
	Copyright © Bryan Apellanes 2015  
*/
using Bam.Net.Incubation;
using System.Reflection;
using System.Text;

namespace Bam.Net.Data
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