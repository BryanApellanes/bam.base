using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Web
{
    public static class Headers
    {
        /// <summary>
        /// The header that identifies the current process using Bam.Net.CoreServices.ApplicationRegistration.Data.ProcessDescriptor.LocalIdentifier.
        /// </summary>
        public static string ProcessLocalIdentifier => "X-Bam-Process-Local-Id";

        /// <summary>
        /// The header that describes the current process using Bam.Net.CoreServices.ApplicationRegistration.Data.ProcessDescriptor.Current.ToString();
        /// </summary>
        public static string ProcessDescriptor => "X-Bam-Process-Descriptor";

        /// <summary>
        /// Gets the process mode as reported by ProcessMode.Current.Mode.
        /// </summary>
        public static string ProcessMode => "X-Bam-Process-Mode";


        public static string ApplicationName => "X-Bam-AppName";

        public static string SecureChannelSessionId => "X-Bam-Secure-Channel-Session-Id";

        [Obsolete("Use SecureChannelSessionId instead")]
        public static string SecureSessionId => "X-Bam-Sps-Session-Id";

        public static string Hash => "X-Bam-Hash";

        public static string Padding => "X-Bam-Padding";

        /// <summary>
        /// Proves that the client knows the shared secret by using 
        /// it to create an hmac value that this header is set to.
        /// </summary>
        public static string Hmac => "X-Bam-Hmac";

        /// <summary>
        /// Header used to request a specific responder on the server
        /// handle a given request.
        /// </summary>
        public static string Responder => "X-Bam-Responder";
    }
}
