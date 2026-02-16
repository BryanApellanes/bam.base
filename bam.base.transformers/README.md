# bam.base.transformers

Value transformation pipeline library providing composable, reversible data transformers for JSON, BSON, and Base64 encoding.

## Overview

bam.base.transformers provides a pipeline-based data transformation system built on the `IValueTransformer<TInput, TOutput>` and `IValueReverseTransformer<TOutput, TInput>` interfaces defined in bam.base. Every transformer has a paired reverse transformer, enabling lossless round-trip encoding and decoding. The library targets .NET 10.0 and depends on bam.base for the core transformer interfaces and on Newtonsoft.Json.Bson for BSON support.

The pipeline architecture is centered on `ValueTransformerPipeline<TData>`, which serializes an object to JSON, converts it to UTF-8 bytes, then passes those bytes through a chain of `IValueTransformer<byte[], byte[]>` stages (such as encryption or compression steps registered via `[PipelineFactoryTransformerName]`). The reverse pipeline (`ValueReverseTransformerPipeline<TData>`) walks the chain in reverse order to reconstruct the original object. `ValueTransformerPipelineFactory` provides a factory that assembles pipelines by name using attribute-based discovery and the `ServiceRegistry` for constructor injection.

The library ships with three built-in transformer pairs: `JsonTransformer<T>` / `JsonReverseTransformer<T>` for JSON string serialization, `BsonTransformer<T>` / `BsonReverseTransformer<T>` for binary JSON encoding, and `Base64Transformer` / `Base64ReverseTransformer` for Base64 string encoding of byte arrays. The `ObjectExtensions` class in this library adds `ToBson()` and `FromBson<T>()` extension methods to any object.

## Key Classes

| Class | Description |
|---|---|
| `ValueTransformer<TInput, TOutput>` | Abstract base for all transformers; implements `IValueTransformer` and `IValueConverter`; provides `Transform()`, `ReverseTransform()`, and `GetReverseTransformer()` |
| `ValueTransformerPipeline<TData>` | Composable pipeline that serializes `TData` to JSON bytes then applies a chain of `byte[] -> byte[]` transformers |
| `ValueReverseTransformerPipeline<TData>` | Reverse pipeline that un-transforms bytes through the chain in reverse order and deserializes back to `TData` |
| `ValueTransformerPipelineFactory` | Factory that builds pipelines from comma-separated transformer names using `[PipelineFactoryTransformerName]` attribute discovery and `ServiceRegistry` for DI |
| `JsonTransformer<TInput>` | Transforms objects to their JSON string representation via `ToJson()` |
| `JsonReverseTransformer<TUntransformed>` | Reverses JSON strings back to typed objects via `FromJson<T>()` |
| `BsonTransformer<TData>` | Transforms objects to BSON byte arrays using `Newtonsoft.Json.Bson` |
| `BsonReverseTransformer<TData>` | Reverses BSON byte arrays back to typed objects |
| `Base64Transformer` | Transforms `byte[]` to Base64 `string` |
| `Base64ReverseTransformer` | Reverses Base64 `string` back to `byte[]` |
| `ObjectExtensions` | Adds `ToBson()` and `FromBson<T>()` extension methods using `BsonDataWriter`/`BsonDataReader` |
| `PipelineFactoryTransformerNameAttribute` | Class-level attribute that assigns a name to a transformer for factory discovery |
| `PipelineFactoryConstructorAttribute` | Constructor-level attribute marking which constructor the factory should use when building a transformer |

## Dependencies

### Package References

| Package | Version |
|---|---|
| Newtonsoft.Json | 13.0.4 |
| Newtonsoft.Json.Bson | 1.0.3 |

### Project References

| Project | Purpose |
|---|---|
| bam.base | Core interfaces (`IValueTransformer`, `IValueReverseTransformer`, `IValueConverter`), extension methods, `ServiceRegistry` |

## Usage Examples

### JSON Round-Trip

```csharp
using Bam.Base.Transformers;

var transformer = new JsonTransformer<MyClass>();

string json = transformer.Transform(myObject);
MyClass restored = transformer.GetReverseTransformer().ReverseTransform(json);
```

### BSON Round-Trip

```csharp
using Bam;

var transformer = new BsonTransformer<MyClass>();

byte[] bson = transformer.Transform(myObject);
MyClass restored = transformer.GetReverseTransformer().ReverseTransform(bson);
```

### Base64 Round-Trip

```csharp
using Bam.Base.Transformers;

var transformer = new Base64Transformer();

string encoded = transformer.Transform(myBytes);
byte[] decoded = transformer.GetReverseTransformer().ReverseTransform(encoded);
```

### BSON Extension Methods

```csharp
using Bam.Base.Transformers;

byte[] bson = myObject.ToBson();
MyClass restored = bson.FromBson<MyClass>();
```

### Pipeline Factory

```csharp
using Bam;
using Bam.DependencyInjection;

var factory = new ValueTransformerPipelineFactory(ServiceRegistry.Create());

// Create a pipeline by naming registered transformers
ValueTransformerPipeline<MyData> pipeline = factory.Create<MyData>("encrypt,compress");

byte[] transformed = pipeline.Transform(myData);
MyData restored = pipeline.GetReverseTransformer().ReverseTransform(transformed);
```

## Known Gaps / Not Yet Implemented

No TODO comments, `NotImplementedException` instances, stub classes, or empty methods were found in this project.
