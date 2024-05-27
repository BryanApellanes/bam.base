namespace Bam.Logging
{
    public interface IEventIdProvider
    {
        int GetEventId(string applicationName, string messageSignature);
    }
}