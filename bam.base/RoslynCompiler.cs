using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
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
            _referenceAssemblyPaths = new HashSet<string>();
            _assembliesToReference = new HashSet<Assembly>();
            OutputKind = OutputKind.DynamicallyLinkedLibrary;
            AssembliesToReference = DefaultAssembliesToReference;
            ReferenceAssemblyResolver = ReferenceAssemblyResolver ?? CoreServices.AssemblyManagement.ReferenceAssemblyResolver.Current;
        }

        public RoslynCompiler(IReferenceAssemblyResolver referenceAssemblyResolver) : this()
        {
            ReferenceAssemblyResolver = referenceAssemblyResolver;
        }
        
        public IReferenceAssemblyResolver ReferenceAssemblyResolver { get; set; } // TOOD: revisit this; this does not work the way it was orignially intended and should be removed.

        readonly HashSet<Assembly> _assembliesToReference;
        public Assembly[] AssembliesToReference
        {
            get => _assembliesToReference.ToArray();
            set
            {
                _assembliesToReference.Clear();
                Array.ForEach(value, a => _assembliesToReference.Add(a));
            }
        }

        readonly HashSet<string> _referenceAssemblyPaths;
        public string[] ReferenceAssemblyPaths => _referenceAssemblyPaths.ToArray();

        public OutputKind OutputKind { get; set; }

        public RoslynCompiler AddAssemblyReference(Type type)
        {
            return AddAssemblyReference(type.Assembly);
        }

        public RoslynCompiler AddAssemblyReference(Assembly assembly)
        {
            _assembliesToReference.Add(assembly);
            return this;
        }

        public RoslynCompiler AddResolvedAssemblyReference(string assemblyName)
        {
            return AddAssemblyReference(ReferenceAssemblyResolver.ResolveReferenceAssemblyPath(assemblyName));
        } 
        
        public RoslynCompiler AddAssemblyReference(string path)
        {
            _referenceAssemblyPaths.Add(path);
            return this;
        }
        
        public RoslynCompiler AddAssemblyReference(FileInfo assemblyFile)
        {
            _assembliesToReference.Add(Assembly.Load(assemblyFile.FullName));
            return this;
        }

        public Assembly CompileAssembly(string assemblyFileName, DirectoryInfo directoryInfo)
        {
            return CompileAssembly(assemblyFileName, directoryInfo.GetFiles("*.cs").ToArray());
        }

        public Assembly CompileAssembly(string assemblyFileName, params FileInfo[] sourceFiles)
        {
            return Assembly.Load(Compile(assemblyFileName, sourceFiles));
        }

        public byte[] Compile(string assemblyFileName, DirectoryInfo directoryInfo)
        {
            return Compile(assemblyFileName, directoryInfo.GetFiles("*.cs").ToArray());
        }
        
        public byte[] Compile(string assemblyFileName, params FileInfo[] sourceFiles)
        {
            return Compile(assemblyFileName, sourceFiles.Select(f => SyntaxFactory.ParseSyntaxTree(f.ReadAllText(), CSharpParseOptions.Default, f.FullName)).ToArray());
        }

        public Assembly CompileAssembly(string assemblyName, string sourceCode, Func<MetadataReference[]> getMetaDataReferences = null)
        {
            return Assembly.Load(Compile(assemblyName, sourceCode, getMetaDataReferences));
        }

        public byte[] Compile(string assemblyName, string sourceCode)
        {
            return Compile(assemblyName, sourceCode, GetMetaDataReferences);
        }
        
        public byte[] Compile(string assemblyName, string sourceCode, params Type[] referenceTypes)
        {
            return Compile(assemblyName, sourceCode, () =>
            {
                MetadataReference[] metadataReferences = GetMetaDataReferences();
                HashSet<MetadataReference> metaDataHashSet = new HashSet<MetadataReference>(metadataReferences);
                foreach(Type referenceType in referenceTypes)
                {
                    metaDataHashSet.Add(MetadataReference.CreateFromFile(referenceType.Assembly.Location));
                }
                return metaDataHashSet.ToArray();
            });
        }

        public byte[] Compile(string assemblyName, string sourceCode, Func<MetadataReference[]> getMetaDataReferences)
        {
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            HashSet<MetadataReference> metadataReferences = new HashSet<MetadataReference>();
            getMetaDataReferences().Each(mdr => metadataReferences.Add(mdr));
            return Compile(assemblyName, () => metadataReferences.ToArray(), tree);
        }

        public byte[] Compile(string assemblyName, params SyntaxTree[] syntaxTrees)
        {
            return Compile(assemblyName, GetMetaDataReferences, syntaxTrees);
        }

        public byte[] Compile(string assemblyName, Func<MetadataReference[]> getMetaDataReferences, params SyntaxTree[] syntaxTrees)
        {
            getMetaDataReferences = getMetaDataReferences ?? GetMetaDataReferences;
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
                    List<Assembly> defaultAssemblies = new List<Assembly>
                    {
                        typeof(DynamicObject).Assembly,
                        typeof(XmlDocument).Assembly,
                        typeof(DataTable).Assembly,
                        typeof(object).Assembly,
                        typeof(JsonWriter).Assembly,
                        typeof(FileInfo).Assembly,
                        typeof(Enumerable).Assembly,
                        Assembly.GetExecutingAssembly()
                    };
                    _defaultAssembliesToReference = defaultAssemblies.ToArray();
                }
                
                return _defaultAssembliesToReference;
            }
        }

        private MetadataReference[] GetMetaDataReferences()
        {
            List<MetadataReference> references = new List<MetadataReference>
            {   
                // Get the path to the mscorlib and private mscorlib
                // libraries that are required for compilation to succeed.
                MetadataReference.CreateFromFile(RuntimeSettings.GetReferenceAssembliesDirectory() + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(RuntimeSettings.GetReferenceAssembliesDirectory() + Path.DirectorySeparatorChar + "netstandard.dll")
            };
            AssemblyName[] referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (AssemblyName referencedAssembly in referencedAssemblies)
            {
                Assembly loadedAssembly = Assembly.Load(referencedAssembly);

                references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
            }
            if(_assembliesToReference.Count > 0)
            {
                _assembliesToReference.Each(assembly => references.Add(MetadataReference.CreateFromFile(assembly.Location)));
            }
            if(_referenceAssemblyPaths.Count > 0)
            {
                _referenceAssemblyPaths.Each(path => references.Add(MetadataReference.CreateFromFile(path)));
            }
            return references.ToArray();
        }
    }
}
