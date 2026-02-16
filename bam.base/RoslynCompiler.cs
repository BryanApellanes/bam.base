using System.Reflection;
using Bam.CoreServices.AssemblyManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Bam
{
    /// <summary>
    /// Compiles C# source code and files into assemblies at runtime using the Roslyn compiler.
    /// Supports configurable metadata references, embedded resources, and output kinds.
    /// </summary>
    public class RoslynCompiler : ICompiler
    {
        private List<FileInfo> _embeddedResourceFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompiler"/> class with default metadata reference resolvers.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompiler"/> class with an additional metadata reference resolver.
        /// </summary>
        /// <param name="metadataReferenceResolver">An additional resolver to include when resolving assembly references.</param>
        public RoslynCompiler(IMetadataReferenceResolver metadataReferenceResolver) : this()
        {
            MetadataReferenceResolver.Resolvers.Add(metadataReferenceResolver);
        }

        /// <summary>
        /// Gets the assembly-path-based metadata reference resolver used internally.
        /// </summary>
        protected AssemblyPathMetadataReferenceResolver AssemblyPathMetadataReferenceResolver { get; }

        /// <summary>
        /// Gets or sets the aggregate metadata reference resolver that combines multiple resolvers for assembly reference resolution.
        /// </summary>
        public AggregateMetadataReferenceResolver MetadataReferenceResolver { get; set; }

        /// <summary>
        /// Gets or sets the output kind (e.g., DLL or EXE) for the compiled assembly. Defaults to <see cref="OutputKind.DynamicallyLinkedLibrary"/>.
        /// </summary>
        public OutputKind OutputKind { get; set; }

        /// <summary>
        /// Adds a file to be included as an embedded resource in the compiled assembly.
        /// </summary>
        /// <param name="file">The file to embed.</param>
        public void AddEmbeddedResourceFile(FileInfo file)
        {
            this._embeddedResourceFiles.Add(file);
        }

        /// <summary>
        /// Adds an assembly at the specified file path as a metadata reference for compilation.
        /// </summary>
        /// <param name="path">The file path of the assembly to reference.</param>
        public void AddReferenceAssembly(string path)
        {
            AssemblyPathMetadataReferenceResolver.AddAssembly(path);
        }

        /// <summary>
        /// Adds multiple assemblies at the specified file paths as metadata references for compilation.
        /// </summary>
        /// <param name="paths">The file paths of the assemblies to reference.</param>
        public void AddReferenceAssemblies(params string[] paths)
        {
            foreach (string path in paths)
            {
                AddReferenceAssembly(path);
            }
        }
        
        /// <summary>
        /// Adds an additional metadata reference resolver to the aggregate resolver used during compilation.
        /// </summary>
        /// <param name="resolver">The metadata reference resolver to add.</param>
        public void AddMetadataReferenceResolver(IMetadataReferenceResolver resolver)
        {
            this.MetadataReferenceResolver.Resolvers.Add(resolver);
        }

        /// <summary>
        /// Compiles all .cs files from the specified directories into a loaded <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assemblyFileName">The name for the resulting assembly.</param>
        /// <param name="directoryInfos">The directories containing .cs source files.</param>
        /// <returns>The compiled and loaded <see cref="Assembly"/>.</returns>
        public Assembly CompileDirectoriesToAssembly(string assemblyFileName, params DirectoryInfo[] directoryInfos)
        {
            return CompileFilesToAssembly(assemblyFileName, directoryInfos.SelectMany(di => di.GetFiles("*.cs")).ToArray());
        }

        /// <summary>
        /// Compiles the specified source files into a loaded <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assemblyFileName">The name for the resulting assembly.</param>
        /// <param name="sourceFiles">The .cs source files to compile.</param>
        /// <returns>The compiled and loaded <see cref="Assembly"/>.</returns>
        public Assembly CompileFilesToAssembly(string assemblyFileName, params FileInfo[] sourceFiles)
        {
            return Assembly.Load(CompileFiles(assemblyFileName, sourceFiles));
        }

        /// <summary>
        /// Compiles all .cs files from the specified directories into a byte array containing the assembly.
        /// </summary>
        /// <param name="assemblyFileName">The name for the resulting assembly.</param>
        /// <param name="directoryInfos">The directories containing .cs source files.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        public byte[] CompileDirectories(string assemblyFileName, params DirectoryInfo[] directoryInfos)
        {
            return CompileFiles(assemblyFileName, directoryInfos.SelectMany(di => di.GetFiles("*.cs")).ToArray());
        }

        /// <summary>
        /// Compiles all .cs files from a single directory into a byte array containing the assembly.
        /// </summary>
        /// <param name="assemblyFileName">The name for the resulting assembly.</param>
        /// <param name="directoryInfo">The directory containing .cs source files.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        public byte[] Compile(string assemblyFileName, DirectoryInfo directoryInfo)
        {
            return CompileFiles(assemblyFileName, directoryInfo.GetFiles("*.cs").ToArray());
        }
        
        /// <summary>
        /// Compiles the specified source files into a byte array containing the assembly.
        /// </summary>
        /// <param name="assemblyFileName">The name for the resulting assembly.</param>
        /// <param name="sourceFiles">The .cs source files to compile.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        public byte[] CompileFiles(string assemblyFileName, params FileInfo[] sourceFiles)
        {
            return Compile(assemblyFileName, sourceFiles.Select(f => SyntaxFactory.ParseSyntaxTree(f.ReadAllText(), CSharpParseOptions.Default, f.FullName)).ToArray());
        }

        /// <summary>
        /// Compiles the specified C# source code string into a loaded <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assemblyName">The name for the resulting assembly.</param>
        /// <param name="sourceCode">The C# source code to compile.</param>
        /// <param name="getMetaDataReferences">Optional function to provide metadata references. Uses default references if null.</param>
        /// <returns>The compiled and loaded <see cref="Assembly"/>.</returns>
        public Assembly CompileAssembly(string assemblyName, string sourceCode, Func<MetadataReference[]>? getMetaDataReferences = null)
        {
            return Assembly.Load(Compile(assemblyName, sourceCode, getMetaDataReferences));
        }

        /// <summary>
        /// Compiles the specified C# source code string into a byte array using the default metadata references.
        /// </summary>
        /// <param name="assemblyName">The name for the resulting assembly.</param>
        /// <param name="sourceCode">The C# source code to compile.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        public byte[] CompileSource(string assemblyName, string sourceCode)
        {
            return Compile(assemblyName, sourceCode, GetMetadataReferences);
        }
        
        /// <summary>
        /// Compiles the specified C# source code string into a byte array, including references to the assemblies of the specified types.
        /// </summary>
        /// <param name="assemblyName">The name for the resulting assembly.</param>
        /// <param name="sourceCode">The C# source code to compile.</param>
        /// <param name="referenceTypes">Types whose assemblies should be added as metadata references.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
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

        /// <summary>
        /// Compiles the specified C# source code string into a byte array using the provided metadata reference function.
        /// </summary>
        /// <param name="assemblyName">The name for the resulting assembly.</param>
        /// <param name="sourceCode">The C# source code to compile.</param>
        /// <param name="getMetaDataReferences">Optional function to provide metadata references. Uses default references if null.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        public byte[] Compile(string assemblyName, string sourceCode, Func<MetadataReference[]>? getMetaDataReferences)
        {
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            return Compile(assemblyName, getMetaDataReferences, tree);
        }

        /// <summary>
        /// Compiles the specified syntax trees into a byte array using the default metadata references and any registered embedded resources.
        /// </summary>
        /// <param name="assemblyName">The name for the resulting assembly.</param>
        /// <param name="syntaxTrees">The parsed syntax trees to compile.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        public byte[] Compile(string assemblyName, params SyntaxTree[] syntaxTrees)
        {
            return Compile(assemblyName, GetMetadataReferences, syntaxTrees);
        }

        /// <summary>
        /// Compiles the specified syntax trees into a byte array using the provided metadata references and any registered embedded resources.
        /// </summary>
        /// <param name="assemblyName">The name for the resulting assembly.</param>
        /// <param name="getMetaDataReferences">Optional function to provide metadata references. Uses default references if null.</param>
        /// <param name="syntaxTrees">The parsed syntax trees to compile.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        public byte[] Compile(string assemblyName, Func<MetadataReference[]>? getMetaDataReferences, params SyntaxTree[] syntaxTrees)
        {
            return Compile(
                assemblyName,
                getMetaDataReferences,
                () => this._embeddedResourceFiles.Select(fileInfo => new ResourceDescription(fileInfo.Name, () => File.OpenRead(fileInfo.FullName), true)),
                syntaxTrees);
        }
        
        /// <summary>
        /// Compiles the specified syntax trees into a byte array using the provided metadata references and embedded resource provider.
        /// This is the core compilation method that all other overloads delegate to.
        /// </summary>
        /// <param name="assemblyName">The name for the resulting assembly.</param>
        /// <param name="getMetaDataReferences">Optional function to provide metadata references. Uses default references if null.</param>
        /// <param name="embeddedResourceProvider">Optional function to provide embedded resources for the assembly. Returns null for no resources if not specified.</param>
        /// <param name="syntaxTrees">The parsed syntax trees to compile.</param>
        /// <returns>A byte array containing the compiled assembly.</returns>
        /// <exception cref="RoslynCompilationException">Thrown when compilation fails, containing the diagnostic messages.</exception>
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
