# bam.base.transformers.tests

Unit tests for the bam.base.transformers library, verifying round-trip correctness of JSON, BSON, and Base64 transformers.

## Overview

bam.base.transformers.tests is an executable test project targeting .NET 10.0 that validates the transformer implementations in bam.base.transformers. It uses the Bam Framework's menu-driven test runner (`BamConsoleContext.StaticMain`) with the `[UnitTest]` attribute and the `When.A<T>()` fluent testing API from bam.test.

The test suite contains a single test class (`TransformerShould`) with three tests covering the core transformer types: BSON round-trip serialization, JSON round-trip serialization, and Base64 round-trip encoding. Each test constructs a transformer instance, transforms input data, reverses the transformation, and asserts that the output matches the original input. The project also references bam.encryption (BouncyCastle) for generating cryptographically secure random bytes in the Base64 test.

A simple `TestMonkey` POCO class with `Name` and `TailCount` properties serves as the test data type for JSON and BSON serialization tests.

## Key Classes

| Class | Description |
|---|---|
| `TransformerShould` | Test class with three unit tests: `TransformBson` (round-trips `TestMonkey` through `BsonTransformer`), `TransformJson` (round-trips through `JsonTransformer`), `TransformBase64` (round-trips random bytes through `Base64Transformer`) |
| `TestMonkey` | Simple test POCO with `Name` (string) and `TailCount` (int) properties used as serialization test data |
| `Program` | Entry point that delegates to `BamConsoleContext.StaticMain(args)` for menu-driven test execution |

## Dependencies

### Project References

| Project | Purpose |
|---|---|
| bam.console | Provides `BamConsoleContext.StaticMain` for the menu-driven test runner |
| bam.encryption | Provides `Org.BouncyCastle.Security.SecureRandom` for generating random bytes in the Base64 test |
| bam.test | Test framework providing `[UnitTest]`, `When.A<T>()`, `UnitTestMenuContainer` |

Note: bam.base.transformers is transitively referenced through bam.test and bam.console dependency chains.

## Usage Examples

### Running the Tests

```bash
# Run all unit tests (use --ut, not /ut in Git Bash)
dotnet run --project submodules/bam.base/bam.base.transformers.tests/bam.base.transformers.tests.csproj -- --ut
```

### Test Structure Pattern

```csharp
[UnitTest]
public void TransformJson()
{
    TestMonkey testMonkey = new TestMonkey { Name = "Fred" };

    When.A<JsonTransformer<TestMonkey>>("round-trips a TestMonkey through JSON",
        (transformer) =>
        {
            string json = transformer.Transform(testMonkey);
            TestMonkey decoded = transformer.GetReverseTransformer().ReverseTransform(json);
            return decoded;
        })
    .TheTest
    .ShouldPass(because =>
    {
        because.TheResult.IsNotNull()
            .As<TestMonkey>("Name equals original", m => testMonkey.Name.Equals(m?.Name));
    })
    .SoBeHappy()
    .UnlessItFailed();
}
```

## Known Gaps / Not Yet Implemented

No TODO comments, `NotImplementedException` instances, stub classes, or empty methods were found in this project.
