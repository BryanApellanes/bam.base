using System;

namespace Bam
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyCommitAttribute : Attribute
    {
        public AssemblyCommitAttribute(string commit)
        {
            Commit = commit;
        }
        
        public string Commit { get; set; }
    }
}