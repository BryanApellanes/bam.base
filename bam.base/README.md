# bam.base

Base foundation library for the Bam Framework, providing dependency injection, serialization, logging, configuration, cryptographic hashing, Roslyn compilation, and a rich set of extension methods.

## Overview

bam.base is the foundational layer of the Bam Framework. It targets .NET 10.0 and lives in the `Bam` root namespace. The library provides the core infrastructure that all other Bam modules depend on: a custom dependency injection container (`ServiceRegistry`/`DependencyProvider`), multi-format serialization (JSON, YAML, XML), a background-threaded logging system, YAML-based configuration management, and a Roslyn-based runtime C# compiler.

The dependency injection system is built around `DependencyProvider`, which maps interface types to concrete implementations via a fluent API (`For<I>().Use<T>()`). `ServiceRegistry` extends this with named registries, validation, and attribute-based auto-discovery via `[ServiceRegistryContainer]` and `[ServiceRegistryLoader]`. The `BamContext` class ties registries to process modes (Dev, Test, Prod), enabling environment-specific configuration.

The library also offers extensive extension methods on `object`, `string`, `byte[]`, `Type`, and other primitives for JSON/YAML/XML serialization and deserialization, hashing (MD5, SHA1, SHA256, SHA384, SHA512, HMAC variants), Base64 encoding, string manipulation (PascalCase, CamelCase, SnakeCase, KabobCase), thread-safe file I/O, process execution, and reflection utilities. An `Expect` assertion class provides a lightweight testing/guard API.

## Key Classes

| Class | Description |
|---|---|
| `DependencyProvider` | Core dependency injection container with type-to-instance mapping, constructor injection, and property injection via `[Inject]` attribute |
| `ServiceRegistry` | Extends `DependencyProvider` with fluent registration API (`For<I>().Use<T>()`), singleton/transient lifetimes, validation, and attribute-based auto-discovery |
| `FluentServiceRegistryContext<I>` | Fluent builder for registering types: `Use<T>()`, `UseSingleton<T>()`, `UseTransient<T>()`, `Returns<T>()` |
| `BamContext` | Abstract application context binding `ServiceRegistry` instances to `ProcessMode` (Dev/Test/Prod); provides `Configure()` for environment-specific DI setup |
| `ProcessMode` | Represents the current process environment (Dev, Test, Prod), resolvable from config, environment variables, or command-line args |
| `ProcessModeServiceRegistry` | Pairs a `ProcessModes` enum value with a `ServiceRegistry` instance |
| `Config` | YAML-based application configuration with file-watching for live reload; reads/writes from `~/.bam/` directory structure |
| `Workspace` | Filesystem abstraction for application-specific working directories under `~/.bam/apps/` |
| `Log` | Static logging facade with `Info`, `Warn`, `Error`, `Debug`, `Trace` methods delegating to an `ILogger` instance |
| `Logger` | Abstract base for logger implementations with background commit thread, verbosity filtering, and event queue |
| `ConsoleLogger` | Default `ILogger` implementation writing to `System.Console` |
| `RoslynCompiler` | Runtime C# compiler using Roslyn (`Microsoft.CodeAnalysis`); compiles source strings, files, or directories to assemblies |
| `Expect` | Static assertion utility: `IsTrue`, `AreEqual`, `IsNotNull`, `Throws`, `ShouldBeGreaterThan`, etc. |
| `Args` | Guard/precondition utility: `ThrowIfNull`, `ThrowIf`, `ThrowIfNullOrEmpty`, and exception message/stack-trace helpers |
| `ObjectExtensions` | Extension methods for `object`: `ToJson()`, `ToYaml()`, `ToXml()`, `CopyProperties()`, `CopyAs<T>()`, `DoubleCheckLock()` |
| `StringExtensions` | Extension methods for `string`: `FromJson<T>()`, `FromYaml<T>()`, `FromXml<T>()`, `SafeWriteFile()`, `Run()` (process execution), case conversions, `Pluralize()` |
| `HashExtensions` | Cryptographic hash extension methods: `Sha256()`, `Md5()`, `HmacSha256HexString()`, `ToHashBytes()`, and HMAC variants |
| `ByteExtensions` | Extension methods for `byte[]`: `ToBase64()`, `ToHexString()`, `ToBase64UrlEncoded()`, `Decode<T>()` |
| `TypeExtensions` | Reflection helpers: `Construct()`, `HasCustomAttributeOfType()`, `GetCustomAttributeOfType()` |
| `ReflectionExtensions` | Type introspection: `ToInfoHash()`, `ToInfoString()`, property/method analysis |
| `Serialization` (internal) | Internal dispatcher routing `SerializationFormat` (Json, Yaml, Xml) to `ISerializer`/`IDeserializer` implementations |
| `Cuid` | CUID (Collision-resistant Unique Identifier) generator in the `NCuid` namespace |
| `Diff` / `DiffReport` | Text diff analytics in the `Analytics` namespace |
| `Loggable` | Base class providing `FireEvent` for event-driven patterns |
| `FileLock` | Named-lock mechanism for thread-safe file I/O |
| `BackgroundThreadQueue` | Generic background thread processing queue |

## Dependencies

### Package References

| Package | Version |
|---|---|
| Microsoft.CodeAnalysis | 5.0.0 |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.1 |
| Newtonsoft.Json | 13.0.4 |
| System.CodeDom | 10.0.1 |
| System.Configuration.ConfigurationManager | 10.0.1 |
| YamlDotNet | 16.3.0 |
| YUICompressor.NET | 3.1.0 |

### Project References

None (this is the base library).

## Usage Examples

### Dependency Injection with ServiceRegistry

```csharp
using Bam.DependencyInjection;

// Create a registry and register types
ServiceRegistry registry = ServiceRegistry.Create()
    .For<IMyService>().Use<MyServiceImpl>()
    .For<ILogger>().Use<ConsoleLogger>();

// Resolve an instance (transient by default)
IMyService service = registry.Get<IMyService>();

// Singleton registration
ServiceRegistry registry2 = ServiceRegistry.Create()
    .For<IMyService>().UseSingleton<MyServiceImpl>();
```

### Environment-Specific Configuration

```csharp
using Bam;
using Bam.DependencyInjection;

// Configure dependencies for the current process mode
BamContext.Configure(svcRegistry =>
    svcRegistry.For<IMyService>().Use<DevService>());

// Configure a specific mode
BamContext.Configure(ProcessModes.Prod, svcRegistry =>
    svcRegistry.For<IMyService>().Use<ProdService>());

// Get the registry for the current mode
ProcessModeServiceRegistry registry = BamContext.GetServiceRegistry();
IMyService svc = registry.ServiceRegistry.Get<IMyService>();
```

### Serialization

```csharp
using Bam;

var data = new { Name = "example", Value = 42 };

// Serialize to JSON
string json = data.ToJson();
data.ToJsonFile("/path/to/file.json");

// Serialize to YAML
string yaml = data.ToYaml();
data.ToYamlFile("/path/to/file.yaml");

// Deserialize
MyClass obj = json.FromJson<MyClass>();
MyClass obj2 = "/path/to/file.yaml".FromYamlFile<MyClass>();
```

### Hashing

```csharp
using Bam;

string hash = "my data".Sha256();
string hmac = "message".HmacSha256HexString("secret-key");
byte[] hashBytes = "data".ToHashBytes(HashAlgorithms.SHA512);
string fileHash = new FileInfo("file.txt").Sha256();
```

### Assertions with Expect

```csharp
using Bam;

Expect.IsNotNull(myObject);
Expect.AreEqual(expected, actual);
Expect.IsTrue(condition, "condition must be true");
Expect.Throws(() => riskyOperation(), "should have thrown");
```

### Runtime Compilation with Roslyn

```csharp
using Bam;

var compiler = new RoslynCompiler();
byte[] assembly = compiler.CompileSource("MyAssembly", @"
    public class Hello {
        public string Greet() => ""Hello, World!"";
    }
");
```

## Known Gaps / Not Yet Implemented

| Location | Description |
|---|---|
| `DependencyProvider.cs` line 34 | `// TODO: implement Circular dependency check` -- Note: `DependencyLoopException` and detection via `HashSet<Type>` in `GetCtorParams` already exists, so this TODO may be partially addressed. |
| `ConsoleLogger.cs` line 10 | `// TODO: break this up into ConsoleLogger and DetailConsoleLogger` |
| `Loggable.cs` line 140 | `// TODO: review this to determine how to properly handle generic EventHandler<TEventArgs>` |
| `StringExtensions.cs` line 911 | `// TODO: obsolete this method` (the private `GetExeAndArguments` helper) |
| `SerializationFormat.cs` line 9 | `// TODO: add ProtoBuf and MessagePack and Bam` serialization formats |
| `Meta.cs` lines 375, 387 | `// TODO: make the algorithm configurable` for hash computation in meta/universal-id resolution |
