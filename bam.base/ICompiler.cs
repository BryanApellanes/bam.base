using Bam.CoreServices.AssemblyManagement;
using System.Reflection;

namespace Bam
{
    public interface ICompiler
    {
        AggregateMetadataReferenceResolver MetadataReferenceResolver { get; set; }
        void AddMetadataReferenceResolver(IMetadataReferenceResolver resolver);

        Assembly CompileDirectoriesToAssembly(string assemblyFileName, params DirectoryInfo[] directoryInfo);
        Assembly CompileFilesToAssembly(string assemblyFileName, params FileInfo[] files);
        byte[] CompileFiles(string assemblyFileName, params FileInfo[] sourceFiles);
        byte[] CompileSource(string assemblyFileName, string sourceCode);        
    }
}
