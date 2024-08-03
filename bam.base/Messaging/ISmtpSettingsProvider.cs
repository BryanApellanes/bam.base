using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Messaging
{
    public interface ISmtpSettingsProvider
    {
        string SmtpSettingsVaultPath { get; set; }
        object GetSmtpSettingsVault(string? applicationName = null);
        /// <summary>
        /// When implemented should return
        /// an email whose smtp settings are already 
        /// set to those of the current ISmtpSettingsProvider
        /// implementation
        /// </summary>
        /// <returns></returns>
        Email CreateEmail(string? fromAddress = null, string? fromDisplayName = null);
    }
}
