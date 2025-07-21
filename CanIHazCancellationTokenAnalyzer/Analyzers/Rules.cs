using Microsoft.CodeAnalysis;

namespace CanIHazCancellationTokenAnalyzer.Analyzers;

public static class Rules
{
    public static readonly DiagnosticDescriptor TaskRule = new("CHIHC001",
        "CancellationToken is missing",
        "Method returning Task/ValueTask should include an optional CancellationToken parameter",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor AsyncVoidRule = new("CHIHC002",
        "CancellationToken is missing",
        "Async void method should include an optional CancellationToken parameter",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor UseRealCancellationTokenRule = new("CHIHC003",
        "Use real CancellationToken instead of CancellationToken.None",
        "Method call should pass a real CancellationToken instead of CancellationToken.None",
        "Usage",
        DiagnosticSeverity.Info,
        true);

    public static readonly DiagnosticDescriptor AddCancellationTokenRule = new("CHIHC004",
        "CancellationToken is missing in method call",
        "Method call should pass a CancellationToken",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor UseOverloadWithCancellationTokenRule = new("CHIHC005",
        "Use overload with CancellationToken",
        "Method call should use an overload that accepts a CancellationToken",
        "Usage",
        DiagnosticSeverity.Warning,
        true);
}
