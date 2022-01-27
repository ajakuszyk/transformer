Transformer
===========

This is a modified version of [Tobias Zuercher transformer project.](https://github.com/tobiaszuercher/transformer)

## How to build

```
cd src/Transformer
dotnet publish -c Release /p:PublishSingleFile=true /p:DebugType=None
```

## Modifications

### You can override variable from transformer file with environment variable

If you define environment variable (e.g. in docker) e.g. application.name and it will take precedence over variable 'application.name' in xml transformer file.

Modified source code:
- [VariableUsage.cs](src/Transformer.Core/Template/VariableUsage.cs)
- [VariableResolverTests](src/Transformer.Tests/VariableResolverTests.cs)

### You can use if empty / if not empty conditionals

```
<!--[if empty ${stringvar}]-->
Natasza!
<!--[endif]-->
```

```
<!--[if not empty ${stringvar}]-->
George Junior!
<!--[endif]-->
```

Modified source code: 
- [VariableResolver.cs](src/Transformer.Core/Template/VariableResolver.cs)
- [VariableResolverTests](src/Transformer.Tests/VariableResolverTests.cs)

### Code migrated to .NET 6

Not quite there yet, as CommandLineParser NuGet package still uses .NET Framework v4.x, but code can be build using dotnet command line.

Added Password generator to replace System.Web dependency in Commands.cs
- [Password.cs](src/Transformer/Password.cs)
- [Commands.cs](src/Transformer/Commands.cs)
