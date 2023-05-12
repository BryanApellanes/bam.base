/*
	Copyright © Bryan Apellanes 2015  
*/
namespace Bam.Net.ServiceProxy
{
    public interface IServiceProxyResponse
    {
        object Data { get; set; }
        string Message { get; set; }
        bool Success { get; set; }

        T DataTo<T>();
    }
}