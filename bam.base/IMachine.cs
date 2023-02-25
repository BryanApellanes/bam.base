using Bam.Net.Data.Repositories;
using System.Collections.Generic;

namespace Bam.Net.CoreServices.ApplicationRegistration.Data
{
    public interface IMachine
    {
        List<Application> Applications { get; set; }
        List<Client> Clients { get; set; }
        List<Configuration> Configurations { get; set; }
        string DnsName { get; set; }
        List<HostAddress> HostAddresses { get; set; }
        string Name { get; set; }
        List<Nic> NetworkInterfaces { get; set; }
        List<ProcessDescriptor> Processes { get; set; }

        string GetFirstMac();
        RepoData Save(IRepository repo);
        string ToJson();
    }
}