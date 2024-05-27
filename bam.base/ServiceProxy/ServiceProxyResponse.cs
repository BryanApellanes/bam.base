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
    public abstract class ServiceProxyResponse : IServiceProxyResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        
        /// <summary>
        /// Relevant data returned in response
        /// to a request
        /// </summary>
        public object Data { get; set; }

        public T DataTo<T>()
        {
            return Data.ToJson().FromJson<T>();
        }
    }
}
