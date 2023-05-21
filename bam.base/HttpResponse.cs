using Bam.Net.ServiceProxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Net.Server
{
    public class HttpResponse : IHttpResponse
    {
        public HttpResponse()
        {
            this.Encoding = Encoding.UTF8;
            this.Content = string.Empty;
        }

        public HttpResponse(string content, int statusCode = 0): this()
        {
            this.Content = content;
            this.StatusCode = statusCode;
        }

        public Encoding Encoding { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }

        public int StatusCode { get; set; }

        public void Send(IResponse response, int statusCode = 0)
        {
            if (statusCode == 0)
            {
                statusCode = StatusCode;
            }
            if (statusCode == 0)
            {
                statusCode = 200;
            }
            if (!string.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }

            byte[] outputData = GetOutput();

            response.StatusCode = statusCode;            
            response.OutputStream.Write(outputData, 0, outputData.Length);
            response.Close();
        }

        protected virtual byte[] GetOutput()
        {
            return Encoding.GetBytes(Content);
        }
    }
}
