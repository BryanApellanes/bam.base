# bam.base

Base of the Bam Framework — core primitives shared by every other `bam.*` library: dependency injection, data-access contracts, logging, serialization, and process/OS utilities.

## Overview

`bam.base` is the foundational, dependency-free layer that the rest of the Bam Toolkit builds on. It has no `ProjectReference`s of its own — everything else in the framework references it, directly or transitively.

It provides the framework's custom dependency-injection container (`ServiceRegistry` / `DependencyProvider`, under `DependencyInjection/`) used in place of `Microsoft.Extensions.DependencyInjection`; the core data-access contracts consumed by `bam.data*` and `bamdb` (`IDao`, `IDaoRepository`, `IQuery`, `IRepository`, schema interfaces under `Data/Schema/`, and code-generation contracts under `Data/Repositories/` such as `IWrapperGenerator` and `ISourceGenerator`); the logging abstractions (`Logging/ILogger`, `Logger`, `LogEntry`, `Loggable`) that back `bam.logging`; pluggable serialization (`Serialization/` — JSON via Newtonsoft, XML, YAML via YamlDotNet); the `ServiceProxy` contracts consumed by `bam.protocol`; and general-purpose primitives — `BamHome`/`BamDir`/`BamProfile` (the `~/.bam` conventions the whole toolkit relies on), `Exec`/`ProcessInfo` (process execution), `RoslynCompiler` (ad-hoc C# compilation), and a large set of extension methods (`StringExtensions`, `ReflectionExtensions`, `EnumerableExtensions`, etc.).

It also embeds two smaller, self-contained sub-projects: `bam.base.transformers` (byte/value transformer pipelines) and `bam.extensions`.

## Key Classes

| Class / Interface | Description |
|---|---|
| `ServiceRegistry` | Fluent DI container (`For<T>().Use<TImpl>()`) used across the entire toolkit instead of a third-party DI framework. |
| `DependencyProvider` | Resolves dependencies registered on a `ServiceRegistry`. |
| `Loggable` | Base class giving any type `ILogger`-backed logging (`Bam.Logging` namespace). |
| `IDao` / `IDaoRepository` / `IRepository` | Core data-access contracts implemented by generated DAOs and consumed by `bam.data*`/`bamdb`. |
| `BamHome` / `BamDir` / `BamProfile` | Resolve the `~/.bam` home directory conventions (build output, packages, profile) used framework-wide. |
| `RoslynCompiler` | Ad-hoc in-memory C# compilation via Roslyn. |
| `Config` | Framework configuration access. |

## Dependencies

**Package References:** `Microsoft.CodeAnalysis`, `Microsoft.Extensions.Configuration.Abstractions`, `Newtonsoft.Json`, `System.CodeDom`, `System.Configuration.ConfigurationManager`, `YamlDotNet`, `YUICompressor.NET`.

**Target Framework:** net10.0, packaged as the `bam.base` NuGet package (Three Headz).

`bam.base` has no project references — it is the root of the framework's dependency graph.

## Running Tests

```bash
dotnet run --project bam.base.tests/bam.base.tests.csproj -- --ut
dotnet run --project bam.base.transformers.tests/bam.base.transformers.tests.csproj -- --ut
```
