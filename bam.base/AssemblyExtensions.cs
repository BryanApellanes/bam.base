using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net
{
    public static class AssemblyExtensions
    {
        public static FileInfo GetFileInfo(this Assembly assembly)
        {
            switch (OSInfo.Current)
            {
                case OSNames.OSX:
                case OSNames.Linux:
                    return new FileInfo(assembly.CodeBase.TruncateFront("file://".Length));
                case OSNames.Windows:
                default:
                    return new FileInfo(assembly.CodeBase.TruncateFront("file:///".Length));
            }

            return new FileInfo(assembly.CodeBase.TruncateFront("file:///".Length));
        }

        public static string GetFilePath(this Assembly assembly)
        {
            return assembly.GetFileInfo().FullName;
        }
    }
}
