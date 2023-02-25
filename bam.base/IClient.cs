namespace Bam.Net.CoreServices.ApplicationRegistration.Data
{
    public interface IClient
    {
        Application Application { get; set; }
        ulong ApplicationId { get; set; }
        ulong ApplicationKey { get; set; }
        string ApplicationName { get; set; }
        Machine Machine { get; set; }
        ulong MachineId { get; set; }
        string MachineName { get; set; }
        int Port { get; set; }
        string Secret { get; set; }
        string ServerHost { get; set; }

        string GetPseudoEmail();
    }
}