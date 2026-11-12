using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Bam
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the file the assembly was loaded from, as reported by <see cref="Assembly.Location"/>.
        /// Returns false for an assembly that has no file: one loaded from bytes (as runtime-generated
        /// Dao and Roslyn-compiled assemblies are), a dynamic assembly, or one bundled in a single-file app.
        /// </summary>
        public static bool TryGetFileInfo(this Assembly assembly, [NotNullWhen(true)] out FileInfo? fileInfo)
        {
            string location = assembly.IsDynamic ? string.Empty : assembly.Location;
            if (string.IsNullOrEmpty(location))
            {
                fileInfo = null;
                return false;
            }

            fileInfo = new FileInfo(location);
            return true;
        }

        /// <summary>
        /// Gets the file the assembly was loaded from. Throws <see cref="InvalidOperationException"/> for an
        /// assembly that has no file (see <see cref="TryGetFileInfo"/>). Use <see cref="TryGetFileInfo"/> where
        /// such assemblies are expected, and <see cref="GetFilePathOrName"/> for log and diagnostic text.
        /// </summary>
        public static FileInfo GetFileInfo(this Assembly assembly)
        {
            if (assembly.TryGetFileInfo(out FileInfo? fileInfo))
            {
                return fileInfo;
            }

            throw new InvalidOperationException($"The assembly ({assembly.FullName}) was not loaded from a file, so it has no file path.");
        }

        /// <summary>
        /// Gets the full path of the file the assembly was loaded from. Throws <see cref="InvalidOperationException"/>
        /// for an assembly that has no file (see <see cref="TryGetFileInfo"/>).
        /// </summary>
        public static string GetFilePath(this Assembly assembly)
        {
            return assembly.GetFileInfo().FullName;
        }

        /// <summary>
        /// Gets the full path of the file the assembly was loaded from or, for an assembly that has no file,
        /// its full name. Never throws; meant for log and diagnostic text.
        /// </summary>
        public static string GetFilePathOrName(this Assembly assembly)
        {
            if (assembly.TryGetFileInfo(out FileInfo? fileInfo))
            {
                return fileInfo.FullName;
            }

            return assembly.FullName ?? assembly.GetName().Name ?? "(unnamed assembly)";
        }
    }
}
