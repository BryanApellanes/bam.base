/*
	Copyright © Bryan Apellanes 2015  
*/

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
