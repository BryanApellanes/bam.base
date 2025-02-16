namespace Bam.ServiceProxy
{
    public interface IServiceProxyResponse<T> : IServiceProxyResponse
    { 
        new T Data { get; set; }
    }
}
