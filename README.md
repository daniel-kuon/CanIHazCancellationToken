# CanIHazCancellationToken 🚫⏱️

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Roslyn Analyzer](https://img.shields.io/badge/Roslyn-Analyzer-purple.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
[![NuGet](https://img.shields.io/nuget/v/CanIHazCancellationToken.svg)](https://www.nuget.org/packages/CanIHazCancellationToken/)

A sophisticated Roslyn analyzer and code fix provider that helps developers properly implement and use
`CancellationToken` in asynchronous C# code. This tool ensures your async methods follow best practices for cancellation
support, improving application responsiveness and resource management.

## 🎯 Purpose

In modern C# applications, proper cancellation token usage is crucial for:

- **Responsive UIs**: Allowing users to cancel long-running operations
- **Resource Management**: Preventing resource leaks in web applications
- **Graceful Shutdowns**: Enabling clean application termination
- **Performance**: Avoiding unnecessary work when operations are no longer needed

This analyzer automatically detects missing or improper `CancellationToken` usage and provides intelligent code fixes.

## 🔍 What It Detects

### 📋 Diagnostic Rules

| Rule ID    | Severity | Description                                                                                  |
|------------|----------|----------------------------------------------------------------------------------------------|
| `CHIHC001` | Warning  | Method returning `Task`/`ValueTask` should include an optional `CancellationToken` parameter |
| `CHIHC002` | Warning  | Method call should pass a `CancellationToken` when an overload is available                  |
| `CHIHC003` | Info     | Use real `CancellationToken` instead of `CancellationToken.None`                             |

## 🛠️ Installation

### NuGet Package Manager

```xml
<PackageReference Include="CanIHazCancellationToken" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### .NET CLI

```bash
dotnet add package CanIHazCancellationToken
```

## 🚀 Features

### 🔧 Intelligent Code Fixes

The analyzer doesn't just detect issues—it provides smart automatic fixes:

1. **Parameter Addition**: Automatically adds `CancellationToken` parameters to method signatures
2. **Token Propagation**: Intelligently passes tokens through method calls
3. **Overload Detection**: Finds method overloads that accept `CancellationToken`
4. **Context Awareness**: Suggests using existing tokens instead of `CancellationToken.None`

## 📖 Examples

### Missing CancellationToken Parameters

```csharp
// ❌ Analyzer Warning (CHIHC001)
public async Task ProcessDataAsync()
{
    await SomeAsyncOperation();
}

// ✅ Fixed
public async Task ProcessDataAsync(CancellationToken cancellationToken = default)
{
    await SomeAsyncOperation(cancellationToken);
}
```

### Missing CancellationToken in Method Calls

```csharp
// ❌ Analyzer Warning (CHIHC002)
public async Task DownloadFileAsync(CancellationToken cancellationToken = default)
{
    await httpClient.GetAsync(url); // Missing cancellationToken
}

// ✅ Fixed
public async Task DownloadFileAsync(CancellationToken cancellationToken = default)
{
    await httpClient.GetAsync(url, cancellationToken);
}
```

## 🔧 Configuration

### EditorConfig Support

Configure rule severity in your `.editorconfig`:

```ini
[*.cs]
# Configure rule severities
dotnet_diagnostic.CHIHC001.severity = warning
dotnet_diagnostic.CHIHC002.severity = warning
dotnet_diagnostic.CHIHC003.severity = suggestion

# Disable specific rules if needed
dotnet_diagnostic.CHIHC001.severity = none
```

## 🤝 Contributing

We welcome contributions! Please see our [contributing guidelines](https://github.com/daniel-kuon/CanIHazCancellationToken/blob/main/CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

## 🔗 Links

- [GitHub Repository](https://github.com/daniel-kuon/CanIHazCancellationToken)
- [NuGet Package](https://www.nuget.org/packages/CanIHazCancellationToken/)
- [Report Issues](https://github.com/daniel-kuon/CanIHazCancellationToken/issues)
- [Release Notes](https://github.com/daniel-kuon/CanIHazCancellationToken/releases)