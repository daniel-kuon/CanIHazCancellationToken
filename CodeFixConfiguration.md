# Code Fix Configuration

This document describes the configuration options available for the CanIHazCancellationToken analyzers and code fix providers.

## PreferUsingStatements Configuration

The `CanIHazCancellationToken.PreferUsingStatements` setting controls how code fix providers generate type references and static member access.

### Configuration Options

- `false` (default): Use fully qualified names
- `true`: Add using statements and use short names

### How to Configure

Add the configuration to your `.editorconfig` file:

```ini
# Use fully qualified names (default behavior)
CanIHazCancellationToken.PreferUsingStatements = false

# Or use short names with using statements
CanIHazCancellationToken.PreferUsingStatements = true
```

### Example Output

#### With PreferUsingStatements = false (default)

```csharp
// Method declaration fix
public async Task MyMethodAsync(System.Threading.CancellationToken cancellationToken = default)
{
    // Method invocation fix
    await SomeMethodAsync(System.Threading.CancellationToken.None);
}
```

#### With PreferUsingStatements = true

```csharp
using System.Threading;

// Method declaration fix
public async Task MyMethodAsync(CancellationToken cancellationToken = default)
{
    // Method invocation fix
    await SomeMethodAsync(CancellationToken.None);
}
```

### Benefits

**Fully Qualified Names (default):**
- No risk of naming conflicts
- More explicit and self-documenting
- No need to manage using statements

**Short Names with Using Statements:**
- More concise and readable code
- Follows common C# coding conventions
- Reduces line length and verbosity

### Implementation Details

The configuration is read from the analyzer config options provided by the Roslyn compiler infrastructure. This ensures compatibility with MSBuild, Visual Studio, and other tooling that supports `.editorconfig` files.

The setting is disabled by default to preserve existing behavior and maintain backward compatibility.