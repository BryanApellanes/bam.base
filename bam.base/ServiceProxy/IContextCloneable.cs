namespace Bam.ServiceProxy
{
    public interface IContextCloneable
    {
        object CloneInContext();
        object Clone(IHttpContext context);
    }
}