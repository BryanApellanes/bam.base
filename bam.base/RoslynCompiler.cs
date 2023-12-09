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
using Bam.Net.CoreServices.AssemblyManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;

namespace Bam.Net
{
    public class RoslynCompiler : ICompiler
    {
        public RoslynCompiler()
        {
            OutputKind = OutputKind.DynamicallyLinkedLibrary;
            MetadataReferenceResolver = new AggregateMetadataReferenceResolver
                (
                    new DefaultMetadataReferenceResolver(),
                    new ReferencedAssemblyMetadataReferenceResolver()
                );
        }

        public RoslynCompiler(IMetadataReferenceResolver metadataReferenceResolver) : this()
        {
            MetadataReferenceResolver.Resolvers.Add(metadataReferenceResolver);
        }

        public AggregateMetadataReferenceResolver MetadataReferenceResolver { get; set; }

        public OutputKind OutputKind { get; set; }

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
            getMetaDataReferences = getMetaDataReferences ?? GetMetadataReferences;
            MetadataReference[] metaDataReferences = getMetaDataReferences();
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(this.OutputKind))
                .AddReferences(metaDataReferences)
                .AddSyntaxTrees(syntaxTrees);
            
            using(MemoryStream stream = new MemoryStream())
            {
                EmitResult compileResult = compilation.Emit(stream); 
                if (!compileResult.Success)
                {
                    throw new RoslynCompilationException(compileResult.Diagnostics);
                }
                return stream.GetBuffer();
            }
        }

        static Assembly[] _defaultAssembliesToReference = new Assembly[] { };
        public static Assembly[] DefaultAssembliesToReference
        {
            get
            {
                if (_defaultAssembliesToReference.Length == 0)
                {
                    HashSet<Assembly> defaultAssemblies = new HashSet<Assembly>
                    {
                        typeof(DynamicObject).Assembly,
                        typeof(XmlDocument).Assembly,
                        typeof(DataTable).Assembly,
                        typeof(object).Assembly,
                        typeof(JsonWriter).Assembly,
                        typeof(Enumerable).Assembly,
                        typeof(MarshalByValueComponent).Assembly,
                        typeof(IComponent).Assembly,
                        typeof(IServiceProvider).Assembly,
                        Assembly.GetExecutingAssembly()
                    };
                    _defaultAssembliesToReference = defaultAssemblies.ToArray();
                }
                
                return _defaultAssembliesToReference;
            }
        }

        private MetadataReference[] GetMetadataReferences()
        {
            return this.MetadataReferenceResolver.GetMetaDataReferences();
        }
    }
}
