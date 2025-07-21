# CA2016 Suppression Feature

This example demonstrates how to configure the CA2016 suppression feature.

## Configuration Options

### 1. Using .editorconfig
Create a `.editorconfig` file with:
```ini
[*]
chihc.suppress_ca2016 = true
```

### 2. Using MSBuild Properties
Add to your .csproj file:
```xml
<PropertyGroup>
    <CHIHCSuppressCA2016>true</CHIHCSuppressCA2016>
</PropertyGroup>
```

## How It Works

When enabled, the CA2016Suppressor will:
1. Check for configuration setting
2. Only suppress CA2016 diagnostics where our CHIHC analyzer would also report issues
3. Provide better, more comprehensive guidance through our analyzer rules

## Example Code

```csharp
public async Task ExampleAsync(CancellationToken cancellationToken = default)
{
    // Without suppression: Both CA2016 and CHIHC004/CHIHC005 would warn
    // With suppression: Only CHIHC004/CHIHC005 warns (CA2016 suppressed)
    await Task.Delay(1000); // Missing cancellationToken parameter
}
```