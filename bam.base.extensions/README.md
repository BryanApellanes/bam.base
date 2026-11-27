# bam.base.extensions

Projects a Bam `ServiceRegistry` into a Microsoft `IServiceCollection`, so an ASP.NET Core host runs with one container (bam.base#58). It's kept apart from `bam.base` for the same reason `bam.logging.extensions` is kept apart from `bam.logging`: the core library takes no Microsoft dependency.

## Why a registry type, not an extension method
To detect conflicts, the projection has to know which bindings were *declared* as replaceable and which arrived some other way (`For<I>().Use`, `Set`, `Include`, `CopyFrom`). A plain `ServiceRegistry` doesn't record that, so `ProjectableServiceRegistry` does. Two bamgateway bugs show what goes wrong without it:
- bamgateway#30: a strict policy evaluator the host registered was silently replaced by the pass-through default, so the gateway failed open.
- bamgateway#55: the host ran one type while `registry.Get` returned another.

## Declarations

| Kind | Call | Projected with |
|---|---|---|
| Seam: one implementation, last declaration wins | `UseSeam<TService, TImplementation>()` | `Replace` (singleton) |
| Catalog: ordered, de-duplicated by type | `AddToCatalog<TService, TImplementation>()`; read with `CatalogTypes<TService>()`, build with `BuildCatalog<TService>()` | `TryAddEnumerable` (singleton) |
| Extension: one more registration of a collection service | `AddExtension(Type, Type)`, `AddExtension(Type, object)` (the same instance twice is one) | `TryAddEnumerable`, or `Add` for an instance |
| Composed: built by the host, never rebound | `Compose<TService, TImplementation>()`, `Compose<TService>(registryFactory, hostFactory?)` | `Replace` (singleton); nothing without a host factory |

A seam or a composed service has **one declaration**. Declaring the same service type as a seam or composed *and* under any other kind (in either order) is a conflict, because the host and `registry.Get` would resolve different types. Catalogs and extensions are collections and may share a service type.

## Projecting
```csharp
ProjectableServiceRegistry registry = new ProjectableServiceRegistry { Name = "Sample" };
registry.UseSeam<IGreeter, PoliteGreeter>()
        .AddToCatalog<IStep, StepA>()
        .Compose<IPipeline, Pipeline>();

registry.ProjectInto(builder.Services, new ProjectionOptions
{
    KnownDefaults = new Dictionary<Type, Type> { [typeof(IGreeter)] = typeof(DefaultGreeter) },
    DeclarationHints = new Dictionary<Type, string> { [typeof(IGreeter)] = "registry.UseGreeter<T>()" },
    DeclarationPlace = "in the AddSample callback",
});
```
`ProjectInto` registers nothing and throws one `InvalidOperationException` naming every conflict when either check finds one:
- **`RegistryConflicts(options)`** finds four things:
  - a service declared as a seam or composed and also under another kind;
  - a declared seam that was rebound or removed on the registry, including through `Include` or `CopyFrom`;
  - a seam named in `DeclarationHints` that is bound without a declaration;
  - a composed binding that was rebound or removed.
- **`SeamConflicts(services, options)`** finds a seam, or a composed binding the registry projects, that the collection already holds as something else. A registration made with a factory or an instance counts, since it has no type to compare. These are not conflicts:
  - the declared type;
  - a `KnownDefaults` entry;
  - a keyed registration.

A composed binding with no host factory is the host's to register and isn't checked against the collection. Nothing is constructed to find a conflict. A seam or composed service that is its own implementation (`UseSeam<Foo, Foo>()`) isn't compared. A registration added to the collection *after* `ProjectInto` isn't checked: `TryAdd` then does nothing, and `Add` wins. Project once: projecting twice into one collection adds each instance extension twice.

## Options

| Option | Meaning |
|---|---|
| `RegistryName` | The name in messages ("The {name} registry …"); defaults to the registry's `Name`. |
| `KnownDefaults` | Service type to the default the host's own registration code adds; never a conflict. |
| `DeclarationHints` | Service type to the call a host should make; such seams are checked even when undeclared. |
| `DeclarationPlace` | Where seams are declared, added to the advice. |
| `SeamConflictAdvice` | A sentence appended to the seam-conflict message. |
| `ComposedAdvice` | Service type to the advice for a rebound composed binding. |
| `RegistryConflictHeader` | What the registry-conflict message says after "The {name} registry"; defaults to "holds a binding the service collection would not get." |
| `DeclarationName` | What an undeclared seam lacks, as in "without a {DeclarationName}"; defaults to "declaration". |

A dictionary set to null reads as empty, and blank wording falls back to the default.
