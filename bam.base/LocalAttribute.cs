namespace Bam
{
    /// <summary>
    /// Used to denote a method that is not 
    /// proxied and executes locally.  Also
    /// allows network invocation of a method if
    /// the service is exposed to the local loopback
    /// address 127.0.0.1.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class LocalAttribute: ExcludeAttribute
    {
    }
}
