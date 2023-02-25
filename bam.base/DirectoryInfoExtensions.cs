using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class DirectoryInfoExtensions
    {
        public static FileInfo[] GetFiles(this DirectoryInfo parent, string[] searchPatterns,
            SearchOption option = SearchOption.TopDirectoryOnly)
        {
            List<FileInfo> results = new List<FileInfo>();
            searchPatterns.Each(spattern => { results.AddRange(parent.GetFiles(spattern, option)); });
            return results.ToArray();
        }
    }
}
