namespace Bam.Services
{
    /// <summary>
    /// Attribute used to decorate a class that contains a 
    /// method used to retrieve a ServiceRegistry
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceRegistryContainerAttribute: Attribute 
    {
    }
}
