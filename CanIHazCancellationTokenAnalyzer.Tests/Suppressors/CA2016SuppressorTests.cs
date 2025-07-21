using System.Threading.Tasks;
using CanIHazCancellationTokenAnalyzer.Suppressors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace CanIHazCancellationTokenAnalyzer.Tests.Suppressors;

public class CA2016SuppressorTests
{
    [Fact]
    public async Task CA2016Suppressor_WhenEnabled_SuppressesCA2016ForMethodsWithoutCancellationToken()
    {
        var test = @"
using System.Threading;
using System.Threading.Tasks;

public class TestClass
{
    public async Task MethodWithTokenAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000); // This would trigger CA2016
    }
}";

        var expected = new DiagnosticResult("CA2016", DiagnosticSeverity.Info)
            .WithSpan(8, 15, 8, 30);

        var suppressed = new SuppressionResult("CHIHCSP001", "CA2016")
            .WithSpan(8, 15, 8, 30);

        await new CSharpSuppressionTest
        {
            TestCode = test,
            ExpectedDiagnostics = { expected },
            ExpectedSuppressions = { suppressed },
            AnalyzerConfigFiles = 
            {
                ("/.globalconfig", "[*]\nchihc.suppress_ca2016 = true")
            }
        }.RunAsync();
    }

    [Fact]
    public async Task CA2016Suppressor_WhenDisabled_DoesNotSuppressCA2016()
    {
        var test = @"
using System.Threading;
using System.Threading.Tasks;

public class TestClass
{
    public async Task MethodWithTokenAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000); // This would trigger CA2016
    }
}";

        var expected = new DiagnosticResult("CA2016", DiagnosticSeverity.Info)
            .WithSpan(8, 15, 8, 30);

        await new CSharpSuppressionTest
        {
            TestCode = test,
            ExpectedDiagnostics = { expected },
            AnalyzerConfigFiles = 
            {
                ("/.globalconfig", "[*]\nchihc.suppress_ca2016 = false")
            }
        }.RunAsync();
    }

    private class CSharpSuppressionTest : CSharpAnalyzerTest<CA2016Suppressor, DefaultVerifier>
    {
        protected override CompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));

            static System.Collections.Generic.ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
            {
                string[] args = { "/warnaserror:nullable" };
                var commandLineArguments = Microsoft.CodeAnalysis.CSharp.CSharpCommandLineParser.Default.Parse(args, baseDirectory: "", sdkDirectory: "");
                return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
            }
        }
    }
}