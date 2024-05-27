/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bam.Logging
{
    public class HashingEventIdProvider : IEventIdProvider
    {
        public virtual int GetEventId(string applicationName, string messageSignature)
        {
            return (applicationName + messageSignature).ToSha1Int();
        }
    }
}
