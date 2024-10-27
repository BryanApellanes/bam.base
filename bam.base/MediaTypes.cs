using System;
using System.Collections.Generic;
using System.Text;

namespace Bam
{
    public class MediaTypes
    {
        public const string Json = "application/json; charset=utf-8";
        public const string BamPipeline = "application/vnd.bam.pipeline";
        public const string AsymmetricCipher = "application/vnd.bam.pipeline+rsa";
        public const string SymmetricCipher = "application/vnd.bam.pipeline+aes";
    }
}
