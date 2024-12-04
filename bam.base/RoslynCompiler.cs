using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Bam.CoreServices.AssemblyManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;

namespace Bam
{
    public class RoslynCompiler : ICompiler
    {
        private List<FileInfo> _embeddedResourceFiles;
        public RoslynCompiler()
        {
            this._embeddedResourceFiles = new List<FileInfo>();
            OutputKind = OutputKind.DynamicallyLinkedLibrary;
            AssemblyPathMetadataReferenceResolver = new AssemblyPathMetadataReferenceResolver();
            MetadataReferenceResolver = new AggregateMetadataReferenceResolver
                (
                    new DefaultMetadataReferenceResolver(),
                    new ReferencedAssemblyMetadataReferenceResolver(),
                    AssemblyPathMetadataReferenceResolver
                );
        }

        public RoslynCompiler(IMetadataReferenceResolver metadataReferenceResolver) : this()
        {
            MetadataReferenceResolver.Resolvers.Add(metadataReferenceResolver);
        }

        protected AssemblyPathMetadataReferenceResolver AssemblyPathMetadataReferenceResolver { get; }
        public AggregateMetadataReferenceResolver MetadataReferenceResolver { get; set; }

        public OutputKind OutputKind { get; set; }

        public void AddEmbeddedResourceFile(FileInfo file)
        {
            this._embeddedResourceFiles.Add(file);
        }

        public void AddReferenceAssembly(string path)
        {
            AssemblyPathMetadataReferenceResolver.AddAssembly(path);
        }

        public void AddReferenceAssemblies(params string[] paths)
        {
            foreach (string path in paths)
            {
                AddReferenceAssembly(path);
            }
        }
        
        public void AddMetadataReferenceResolver(IMetadataReferenceResolver resolver)
        {
            this.MetadataReferenceResolver.Resolvers.Add(resolver);
        }

        public Assembly CompileDirectoriesToAssembly(string assemblyFileName, params DirectoryInfo[] directoryInfos)
        {
            return CompileFilesToAssembly(assemblyFileName, directoryInfos.SelectMany(di => di.GetFiles("*.cs")).ToArray());
        }

        public Assembly CompileFilesToAssembly(string assemblyFileName, params FileInfo[] sourceFiles)
        {
            return Assembly.Load(CompileFiles(assemblyFileName, sourceFiles));
        }

        public byte[] CompileDirectories(string assemblyFileName, params DirectoryInfo[] directoryInfos)
        {
            return CompileFiles(assemblyFileName, directoryInfos.SelectMany(di => di.GetFiles("*.cs")).ToArray());
        }

        public byte[] Compile(string assemblyFileName, DirectoryInfo directoryInfo)
        {
            return CompileFiles(assemblyFileName, directoryInfo.GetFiles("*.cs").ToArray());
        }
        
        public byte[] CompileFiles(string assemblyFileName, params FileInfo[] sourceFiles)
        {
            return Compile(assemblyFileName, sourceFiles.Select(f => SyntaxFactory.ParseSyntaxTree(f.ReadAllText(), CSharpParseOptions.Default, f.FullName)).ToArray());
        }

        public Assembly CompileAssembly(string assemblyName, string sourceCode, Func<MetadataReference[]>? getMetaDataReferences = null)
        {
            return Assembly.Load(Compile(assemblyName, sourceCode, getMetaDataReferences));
        }

        public byte[] CompileSource(string assemblyName, string sourceCode)
        {
            return Compile(assemblyName, sourceCode, GetMetadataReferences);
        }
        
        public byte[] Compile(string assemblyName, string sourceCode, params Type[] referenceTypes)
        {
            return Compile(assemblyName, sourceCode, () =>
            {
                MetadataReference[] metadataReferences = GetMetadataReferences();
                HashSet<MetadataReference> metaDataHashSet = new HashSet<MetadataReference>(metadataReferences);
                foreach(Type referenceType in referenceTypes)
                {
                    metaDataHashSet.Add(MetadataReference.CreateFromFile(referenceType.Assembly.Location));
                }
                return metaDataHashSet.ToArray();
            });
        }

        public byte[] Compile(string assemblyName, string sourceCode, Func<MetadataReference[]>? getMetaDataReferences)
        {
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            return Compile(assemblyName, getMetaDataReferences, tree);
        }

        public byte[] Compile(string assemblyName, params SyntaxTree[] syntaxTrees)
        {
            return Compile(assemblyName, GetMetadataReferences, syntaxTrees);
        }

        public byte[] Compile(string assemblyName, Func<MetadataReference[]>? getMetaDataReferences, params SyntaxTree[] syntaxTrees)
        {
            return Compile(
                assemblyName,
                getMetaDataReferences,
                () => this._embeddedResourceFiles.Select(fileInfo => new ResourceDescription(fileInfo.Name, () => File.OpenRead(fileInfo.FullName), true)),
                syntaxTrees);
        }
        
        public byte[] Compile(string assemblyName, Func<MetadataReference[]>? getMetaDataReferences, Func<IEnumerable<ResourceDescription>?>? embeddedResourceProvider, params SyntaxTree[] syntaxTrees)
        {
            getMetaDataReferences = getMetaDataReferences ?? GetMetadataReferences;
            embeddedResourceProvider = embeddedResourceProvider ?? (() => null);
            
            MetadataReference[] metaDataReferences = getMetaDataReferences();
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(this.OutputKind))
                .AddReferences(metaDataReferences)
                .AddSyntaxTrees(syntaxTrees);
            
            using(MemoryStream stream = new MemoryStream())
            {
                EmitResult compileResult = compilation.Emit(stream, manifestResources: embeddedResourceProvider()); 
                if (!compileResult.Success)
                {
                    throw new RoslynCompilationException(compileResult.Diagnostics);
                }
                return stream.GetBuffer();
            }
        }

        private MetadataReference[] GetMetadataReferences()
        {
            return this.MetadataReferenceResolver.GetMetaDataReferences();
        }
    }
}
