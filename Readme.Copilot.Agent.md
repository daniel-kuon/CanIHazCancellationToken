# CanIHazCancellationToken 🚫⏱️

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Roslyn Analyzer](https://img.shields.io/badge/Roslyn-Analyzer-purple.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)

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

### 🎯 Target Scenarios

#### 1. Missing CancellationToken Parameters

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

#### 2. Missing CancellationToken in Method Calls

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

#### 3. Using CancellationToken.None

```csharp
// ⚠️ Analyzer Info (CHIHC003)
public async Task ProcessAsync(CancellationToken cancellationToken = default)
{
    await SomeOperation(CancellationToken.None); // Should use the provided token
}

// ✅ Better
public async Task ProcessAsync(CancellationToken cancellationToken = default)
{
    await SomeOperation(cancellationToken);
}
```

## 🚀 Features

### 🔧 Intelligent Code Fixes

The analyzer doesn't just detect issues—it provides smart automatic fixes:

1. **Parameter Addition**: Automatically adds `CancellationToken` parameters to method signatures
2. **Token Propagation**: Intelligently passes tokens through method calls
3. **Overload Detection**: Finds method overloads that accept `CancellationToken`
4. **Context Awareness**: Suggests using existing tokens instead of `CancellationToken.None`

### 🎯 Supported Method Types

- `async Task` methods
- `async Task<T>` methods
- `async ValueTask` methods
- `async ValueTask<T>` methods
- `async void` methods (with warnings)
- Non-async methods returning `Task`/`ValueTask`

### 🔍 Advanced Analysis

- **Semantic Analysis**: Uses Roslyn's semantic model for accurate type detection
- **Overload Resolution**: Intelligently detects available overloads with `CancellationToken`
- **Method Chain Analysis**: Traces cancellation tokens through method call chains
- **Generic Type Support**: Handles generic `Task<T>` and `ValueTask<T>` return types

## 📦 Project Structure

```
CanIHazCancellationToken/
├── 📁 CanIHazCancellationTokenAnalyzer/          # Main analyzer project
│   ├── 📁 Analyzers/
│   │   ├── 📄 MethodInvocationAnalyzer.cs        # Analyzes method calls
│   │   └── 📄 TaskMethodAnalyzer.cs              # Analyzes method declarations
│   ├── 📁 CodeFixes/
│   │   ├── 📄 MethodInvocationFixProvider.cs     # Fixes method calls
│   │   ├── 📄 TaskMethodFixProvider.cs           # Fixes method signatures
│   │   └── 📄 UseNoneCancellationFixProvider.cs  # Fixes CancellationToken.None usage
│   └── 📄 CanIHazCancellationTokenAnalyzer.csproj
├── 📁 CanIHazCancellationTokenAnalyzer.Tests/    # Comprehensive test suite
│   ├── 📁 Analyzers/                             # Analyzer tests
│   ├── 📁 CodeFixes/                             # Code fix tests
│   └── 📁 Utils/                                 # Test utilities
├── 📁 CanIHazCancellationTokenSample/            # Sample/demo project
└── 📄 CanIHazCancellationToken.sln               # Solution file
```

## 🛠️ Installation & Usage

### Visual Studio / Rider Integration

1. **Build the analyzer**:
   ```bash
   dotnet build CanIHazCancellationTokenAnalyzer
   ```

2. **Install as NuGet package** (when published):
   ```xml
   <PackageReference Include="CanIHazCancellationTokenAnalyzer" Version="1.0.0">
     <PrivateAssets>all</PrivateAssets>
     <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
   </PackageReference>
   ```

3. **Project reference** (for development):
   ```xml
   <ProjectReference Include="path\to\CanIHazCancellationTokenAnalyzer.csproj">
     <OutputItemType>Analyzer</OutputItemType>
     <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
   </ProjectReference>
   ```

### Command Line Usage

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Analyze a specific project
dotnet build YourProject.csproj --verbosity normal
```

## 🧪 Testing

The project includes a comprehensive test suite covering:

- **Analyzer Accuracy**: Ensures correct detection of issues
- **Code Fix Correctness**: Validates that fixes produce correct code
- **Edge Cases**: Tests various scenarios and method signatures
- **Performance**: Ensures analyzer doesn't impact build times significantly

Run the tests:

```bash
cd CanIHazCancellationTokenAnalyzer.Tests
dotnet test --logger trx --results-directory TestResults
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

### MSBuild Integration

Control analyzer behavior in your project file:

```xml

<PropertyGroup>
    <!-- Enable/disable specific analyzers -->
    <WarningsAsErrors>CHIHC001;CHIHC002</WarningsAsErrors>
    <WarningsNotAsErrors>CHIHC003</WarningsNotAsErrors>
</PropertyGroup>
```

## 📦 Version Management

This project follows strict version management policies to ensure stability and security:

- **No Automatic Downgrades**: Package versions and .NET framework targets are never automatically downgraded
- **Version Consistency**: All projects maintain consistent minimum version requirements  
- **Security First**: Only stable, secure versions are used unless explicitly documented
- **Reproducible Builds**: Package lock files ensure consistent dependency resolution

See [VERSION_MANAGEMENT_POLICY.md](VERSION_MANAGEMENT_POLICY.md) for detailed guidelines.

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. **🐛 Report Issues**: Found a bug or false positive? Open an issue
2. **💡 Suggest Features**: Have ideas for new rules? We'd love to hear them
3. **🔧 Submit PRs**: Fix bugs or add features
4. **📖 Improve Documentation**: Help make the docs better
5. **🧪 Add Tests**: More test coverage is always appreciated

### Version Management Guidelines
- Never downgrade package or framework versions without explicit justification
- Update minimum versions in `Directory.Build.props` when upgrading dependencies
- Run version validation: `dotnet build` will check for version consistency

### Development Setup

```bash
# Clone the repository
git clone https://github.com/daniel-kuon/CanIHazCancellationToken.git

# Build the solution
cd CanIHazCancellationToken
dotnet build

# Run tests
dotnet test

# Test with sample project
cd CanIHazCancellationTokenSample
dotnet build  # Should show analyzer warnings
```

## 📊 Performance Impact

- **Build Time**: Minimal impact (< 5% increase in typical projects)
- **Memory Usage**: Low memory footprint
- **IDE Integration**: Fast analysis with real-time feedback
- **Scalability**: Efficiently handles large codebases

## 🔗 Related Resources

- [Cancellation in Managed Threads](https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Cancellation Token Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/cancellation-in-async-tasks)
- [Roslyn Analyzer Development](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)
- [Writing High-Performance C#](https://docs.microsoft.com/en-us/dotnet/csharp/advanced-topics/performance/)

## 📄 License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

## 🙏 Acknowledgments

- **Roslyn Team**: For the excellent analyzer SDK
- **C# Community**: For feedback and feature requests
- **Contributors**: Everyone who has helped improve this project

---

<div align="center">

**Made with ❤️ for the C# community**

[Report Bug](../../issues) • [Request Feature](../../issues) • [Discussions](../../discussions)

</div>
