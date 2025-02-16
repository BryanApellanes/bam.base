/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Messaging
{
    public interface IEmailComposer
    {        
        Email Compose(string subject, string emailName, params object[] data);
        void SetEmailTemplate(string emailName, FileInfo file);
        void SetEmailTemplate(string emailName, string templateContent, bool isHtml = false);
        string GetEmailBody(string emailName, params object[] data);
    }
}
