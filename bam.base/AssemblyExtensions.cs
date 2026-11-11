using System.Reflection;

namespace Bam
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the file the assembly was loaded from. Throws <see cref="ArgumentException"/> for an assembly
        /// that has no file (dynamic, loaded from bytes, or bundled in a single-file app), whose
        /// <see cref="Assembly.Location"/> is empty.
        /// </summary>
        public static FileInfo GetFileInfo(this Assembly assembly)
        {
            return new FileInfo(assembly.Location);
        }

        public static string GetFilePath(this Assembly assembly)
        {
            return assembly.GetFileInfo().FullName;
        }
    }
}
