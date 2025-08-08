# 🚀 CanIHazCancellationToken - Advanced C# Analyzer

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Roslyn Analyzer](https://img.shields.io/badge/Roslyn-Analyzer-purple.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)

> **A sophisticated Roslyn analyzer that ensures proper CancellationToken usage in your C# codebase, promoting better async/await patterns and cancellation support.**

## 🎯 Overview

`CanIHazCancellationToken` is a powerful static code analysis tool designed to enforce best practices around `CancellationToken` usage in C# applications. In modern asynchronous programming, proper cancellation support is crucial for building responsive and resource-efficient applications. This analyzer helps developers identify missing cancellation tokens, improper usage patterns, and provides automated fixes to improve code quality.

### 🌟 Key Features

- **🔍 Comprehensive Analysis**: Detects missing CancellationToken parameters in async methods
- **⚡ Smart Detection**: Identifies method calls that should pass CancellationToken but don't
- **🛠️ Automated Fixes**: Provides intelligent code fixes with multiple resolution strategies
- **📊 Multiple Diagnostic Rules**: Three distinct analyzers covering different scenarios
- **🎨 IDE Integration**: Seamless integration with Visual Studio, VS Code, and JetBrains Rider
- **⚙️ Configurable**: Customizable severity levels and rule configurations

## 📋 Diagnostic Rules

### CHIHC001 - Missing CancellationToken Parameter
**Severity**: Warning  
**Category**: Usage

Detects methods that return `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`, or are `async void` but lack a `CancellationToken` parameter.

```csharp
// ❌ Triggers CHIHC001
public async Task ProcessDataAsync()
{
    await SomeOperationAsync();
}

// ✅ Correct implementation
public async Task ProcessDataAsync(CancellationToken cancellationToken = default)
{
    await SomeOperationAsync(cancellationToken);
}
```

### CHIHC002 - Missing CancellationToken in Method Call
**Severity**: Warning  
**Category**: Usage

Identifies method invocations that have overloads accepting `CancellationToken` but don't pass one.

```csharp
// ❌ Triggers CHIHC002
public async Task ExampleAsync(CancellationToken cancellationToken)
{
    await Task.Delay(1000); // Missing cancellationToken
}

// ✅ Correct implementation
public async Task ExampleAsync(CancellationToken cancellationToken)
{
    await Task.Delay(1000, cancellationToken);
}
```

### CHIHC003 - Avoid CancellationToken.None
**Severity**: Info  
**Category**: Usage

Detects usage of `CancellationToken.None` when a real cancellation token should be used.

```csharp
// ❌ Triggers CHIHC003
public async Task ProcessAsync(CancellationToken cancellationToken)
{
    await SomeServiceAsync(CancellationToken.None);
}

// ✅ Correct implementation
public async Task ProcessAsync(CancellationToken cancellationToken)
{
    await SomeServiceAsync(cancellationToken);
}
```

## 🚀 Installation

### Package Manager Console
```powershell
Install-Package CanIHazCancellationTokenAnalyzer
```

### .NET CLI
```bash
dotnet add package CanIHazCancellationTokenAnalyzer
```

### PackageReference
```xml
<PackageReference Include="CanIHazCancellationTokenAnalyzer" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

## 💡 Usage Examples

### Basic Async Method Pattern
```csharp
// Before: Missing cancellation support
public class DataService
{
    public async Task<string> FetchDataAsync()
    {
        using var client = new HttpClient();
        return await client.GetStringAsync("https://api.example.com/data");
    }
}

// After: With proper cancellation support
public class DataService
{
    public async Task<string> FetchDataAsync(CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync("https://api.example.com/data", cancellationToken);
    }
}
```

### Repository Pattern with Cancellation
```csharp
public interface IUserRepository
{
    Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(User user, CancellationToken cancellationToken = default);
}

public class UserRepository : IUserRepository
{
    private readonly DbContext _context;

    public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

## 🛠️ Code Fixes

The analyzer provides intelligent automated fixes:

### 1. **Pass Existing Token**
When a `CancellationToken` parameter exists in the current method, the fix will automatically pass it to the method call.

### 2. **Add Parameter and Use**
When no `CancellationToken` parameter exists, the fix will:
- Add an optional `CancellationToken` parameter to the method
- Pass the new parameter to the flagged method call

### 3. **Multiple Token Selection**
When multiple `CancellationToken` parameters exist, the fix provides options to choose which token to pass.

### 4. **Replace CancellationToken.None**
Automatically replaces `CancellationToken.None` with the appropriate cancellation token from the method signature.

## ⚙️ Configuration

### EditorConfig Support
```ini
# .editorconfig
[*.cs]

# Configure rule severity
dotnet_diagnostic.CHIHC001.severity = warning
dotnet_diagnostic.CHIHC002.severity = warning
dotnet_diagnostic.CHIHC003.severity = suggestion

# Disable specific rules if needed
dotnet_diagnostic.CHIHC001.severity = none
```

### Global AnalyzerConfig
```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <WarningsAsErrors />
    <WarningsNotAsErrors>CHIHC003</WarningsNotAsErrors>
  </PropertyGroup>
</Project>
```

## 🏗️ Architecture

### Project Structure
```
CanIHazCancellationTokenAnalyzer/
├── Analyzers/
│   ├── MethodInvocationAnalyzer.cs    # CHIHC002, CHIHC003
│   └── TaskMethodAnalyzer.cs          # CHIHC001
├── CodeFixes/
│   ├── MethodInvocationFixProvider.cs
│   ├── TaskMethodFixProvider.cs
│   └── UseNoneCancellationFixProvider.cs
└── Properties/
    └── AssemblyInfo.cs
```

### Technical Details

- **Target Framework**: .NET Standard 2.0
- **Language Version**: Latest C#
- **Dependencies**: Microsoft.CodeAnalysis 4.13.0
- **Concurrent Execution**: Enabled for performance
- **Generated Code**: Analysis disabled by default

## 🧪 Testing

The project includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories
- **Analyzer Tests**: Verify diagnostic detection
- **CodeFix Tests**: Ensure fixes work correctly
- **Edge Case Tests**: Handle complex scenarios
- **Performance Tests**: Validate analyzer efficiency

## 🤝 Contributing

We welcome contributions! Here's how to get started:

### Development Setup
1. **Clone the repository**
   ```bash
   git clone https://github.com/daniel-kuon/CanIHazCancellationToken.git
   cd CanIHazCancellationToken
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run tests**
   ```bash
   dotnet test
   ```

### Contribution Guidelines

- **Code Style**: Follow existing patterns and use EditorConfig
- **Testing**: Add tests for new features and bug fixes
- **Documentation**: Update README and XML documentation
- **Performance**: Ensure analyzers remain fast and efficient

### Reporting Issues
- Use GitHub Issues for bug reports and feature requests
- Provide minimal reproduction cases
- Include relevant code samples and error messages

## 📊 Performance Considerations

- **Incremental Analysis**: Supports incremental compilation
- **Concurrent Execution**: Analyzers run in parallel
- **Memory Efficient**: Minimal allocations during analysis
- **Fast Startup**: Quick initialization and low overhead

## 🔧 Advanced Usage

### Custom Diagnostic Suppressions
```csharp
[SuppressMessage("Usage", "CHIHC001:CancellationToken is missing")]
public Task LegacyMethodAsync()
{
    return Task.CompletedTask;
}
```

### Conditional Compilation
```csharp
#if !DISABLE_CANCELLATION_ANALYSIS
public async Task ProcessAsync(CancellationToken cancellationToken = default)
#else
public async Task ProcessAsync()
#endif
{
    // Implementation
}
```

## 📚 Best Practices

1. **Always Accept CancellationToken**: Make it optional with `default` value
2. **Propagate Tokens**: Pass tokens through the call chain
3. **Check for Cancellation**: Use `cancellationToken.ThrowIfCancellationRequested()`
4. **Timeout Patterns**: Combine with `CancellationTokenSource.CreateLinkedTokenSource()`
5. **UI Responsiveness**: Use cancellation in long-running operations

## 🔗 Related Resources

- [Microsoft Docs: Cancellation in Managed Threads](https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Roslyn Analyzer Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2025 daniel-kuon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

## 🙏 Acknowledgments

- **Microsoft Roslyn Team** - For the excellent analyzer framework
- **Community Contributors** - For feedback and improvements
- **Open Source Community** - For inspiration and best practices

---

<div align="center">

**Made with ❤️ for better C# code quality**

[Report Bug](https://github.com/daniel-kuon/CanIHazCancellationToken/issues) • [Request Feature](https://github.com/daniel-kuon/CanIHazCancellationToken/issues) • [Documentation](https://github.com/daniel-kuon/CanIHazCancellationToken/wiki)

</div>
