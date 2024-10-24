﻿using Bam.ServiceProxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bam.Server
{
    public interface IHttpResponse
    {
        Encoding Encoding { get; set; }

        string Content { get; set; }

        int StatusCode { get; set; }

        void Send(IResponse response, int statusCode = 0);
    }
}
