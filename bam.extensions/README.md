# bam.extensions

Placeholder library targeting .NET Standard 2.1 for cross-platform extension methods.

## Overview

bam.extensions is a class library targeting `netstandard2.1`, making it compatible with both .NET Framework and .NET Core/.NET 5+ runtimes. Unlike the other projects in the bam.base solution which target `net10.0`, this project uses the broader .NET Standard target for maximum compatibility.

The project currently contains no source files. It exists as an empty project skeleton with only the `.csproj` file present. It has no package references, no project references, and no C# source files. Based on its name and target framework, it appears to be reserved as a future home for extension methods that need to be shared across projects targeting different .NET runtime versions, where `netstandard2.1` compatibility is required.

The project has nullable reference types enabled but does not enable implicit usings (unlike the `net10.0` projects in the solution).

## Key Classes

This project contains no classes. It is currently an empty project.

## Dependencies

### Package References

None.

### Project References

None.

## Usage Examples

No public API is currently available. This project is empty.

## Known Gaps / Not Yet Implemented

| Location | Description |
|---|---|
| Entire project | The project contains no source files. It appears to be a placeholder for future extension methods requiring .NET Standard 2.1 compatibility. |
