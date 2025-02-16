using System.Reflection;

namespace Bam
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

        public static Assembly ToAssembly(this DirectoryInfo directory, string assemblyFileName, out byte[] bytes, params string[] referenceAssemblies)
        {
            RoslynCompiler compiler = new RoslynCompiler();
            compiler.AddReferenceAssemblies(referenceAssemblies);
            bytes = compiler.CompileDirectories(assemblyFileName, directory);
            return Assembly.Load(bytes);
        }
        
        public static Assembly ToAssembly(this DirectoryInfo directory, string assemblyFileName, params string[] referenceAssemblies)
        {
            return ToAssembly(directory, assemblyFileName, out _, referenceAssemblies);
        }
    }
}
