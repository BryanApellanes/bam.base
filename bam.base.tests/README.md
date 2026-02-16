# bam.base.tests

Unit tests for the bam.base library, covering dependency injection, context management, hashing, and runtime configuration.

## Overview

bam.base.tests is an executable test project targeting .NET 10.0 that validates the core functionality of the bam.base library. It uses the Bam Framework's own menu-driven test runner (`BamConsoleContext.StaticMain`) rather than xUnit or NUnit. Tests are organized using the `[UnitTestMenu]` attribute for grouping and the `[UnitTest]` attribute for individual test methods.

The test suite focuses on the `ServiceRegistry` dependency injection container (singleton vs transient lifetime, type replacement, constructor injection, and dependency loop detection), the `BamContext` process-mode configuration system (per-mode registry configuration and resolution), cryptographic hash round-tripping, and `RuntimeConfig` default generation. Test helper classes in the `TestClasses` subdirectory provide dependency loop scenarios (circular constructor references between `ClassA`/`ClassB` and a wider loop through `FirstClass`/`SecondClass`/`ThirdClass`).

Tests use the fluent `When.A<T>()` API from `bam.test`, which constructs a subject, executes an action, and asserts results via `.ShouldPass(because => ...)` callbacks. This pattern provides descriptive output with natural-language test names and structured pass/fail reporting.

## Key Classes

| Class | Description |
|---|---|
| `ServiceRegistryShould` | Tests `ServiceRegistry` features: singleton vs transient resolution, type replacement, constructor auto-injection, and dependency loop detection (`DependencyLoopException`) |
| `BamContextShould` | Tests `BamContext` configuration: retrieving the service registry for the current `ProcessMode`, configuring different loggers per mode, and scoping registrations to the current mode only |
| `HashShould` | Tests hash round-tripping: hashes a random string with SHA256, converts to hex, converts back to bytes, and verifies byte equality |
| `RuntimeConfigShould` | Tests `RuntimeConfig.WriteDefault()` produces a non-null result |
| `TestClass` | Simple implementation of `ITestClass` returning a random name (used as the default DI target) |
| `DifferentTestClass` | Alternative `ITestClass` implementation used to test type replacement in `ServiceRegistry` |
| `DependentClass` | Class with a constructor taking `ITestClass`, used to verify constructor injection with the replaced type |
| `NotThisTestClass` | Implementation of `ITestClass` that throws `NotImplementedException` from `Name` (used as a negative test fixture) |
| `ClassA` / `ClassB` | Tight circular dependency pair for dependency loop detection tests |
| `FirstClass` / `SecondClass` / `ThirdClass` | Wide circular dependency chain for dependency loop detection tests |

## Dependencies

### Project References

| Project | Purpose |
|---|---|
| bam.base | The library under test |
| bam.test | Test framework providing `[UnitTestMenu]`, `[UnitTest]`, `When.A<T>()`, `UnitTestMenuContainer` |
| bam.console | Provides `BamConsoleContext.StaticMain` for the menu-driven test runner |
| bam.configuration | Configuration support |
| bam.data | Data access support |
| bam.logging | Logging infrastructure |
| bam.shell | Shell/menu infrastructure |

## Usage Examples

### Running the Tests

```bash
# Run all unit tests (use --ut, not /ut in Git Bash)
dotnet run --project submodules/bam.base/bam.base.tests/bam.base.tests.csproj -- --ut
```

### Test Structure Pattern

Tests follow the `When.A<T>()` fluent pattern:

```csharp
[UnitTestMenu("ServiceRegistry Should", Selector = "srs")]
public class ServiceRegistryShould
{
    [UnitTest]
    public void UseSingleton()
    {
        When.A<ServiceRegistry>("uses singleton registration",
            () => ServiceRegistry.Create().For<ITestClass>().UseSingleton<TestClass>(),
            (svcRegistry) =>
            {
                ITestClass refOne = svcRegistry.Get<ITestClass>();
                ITestClass refTwo = svcRegistry.Get<ITestClass>();
                return new object[] { refOne, refTwo };
            })
        .TheTest
        .ShouldPass(because =>
        {
            object[] results = (object[])because.Result;
            ITestClass refOne = (ITestClass)results[0];
            ITestClass refTwo = (ITestClass)results[1];
            because.ItsTrue("references are the same", ReferenceEquals(refOne, refTwo));
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
```

## Known Gaps / Not Yet Implemented

| Location | Description |
|---|---|
| `NotThisTestClass.cs` line 5 | `Name` property throws `NotImplementedException` -- intentional negative test fixture, not a gap |

No significant gaps or TODO comments were found in the test project itself.
