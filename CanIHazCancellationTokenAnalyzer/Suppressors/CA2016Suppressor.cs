using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CanIHazCancellationTokenAnalyzer.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CA2016Suppressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor SuppressCA2016 = new(
        id: "CHIHCSP001",
        suppressedDiagnosticId: "CA2016",
        justification: "CancellationToken usage is handled by CanIHazCancellationToken analyzer");

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => [SuppressCA2016];

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        // Check if CA2016 suppression is enabled through configuration
        if (!IsCA2016SuppressionEnabled(context))
        {
            return;
        }

        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            if (diagnostic.Id == "CA2016" && ShouldSuppressCA2016(diagnostic, context))
            {
                var suppression = Suppression.Create(SuppressCA2016, diagnostic);
                context.ReportSuppression(suppression);
            }
        }
    }

    private static bool IsCA2016SuppressionEnabled(SuppressionAnalysisContext context)
    {
        // Check global analyzer config options for the setting
        var globalOptions = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
        if (globalOptions.TryGetValue("chihc.suppress_ca2016", out var configValue))
        {
            if (bool.TryParse(configValue, out var result))
                return result;
        }
        
        // Check MSBuild property as fallback
        if (globalOptions.TryGetValue("build_property.chihcsuppressca2016", out var msbuildValue))
        {
            if (bool.TryParse(msbuildValue, out var result))
                return result;
        }

        return false;
    }

    private static bool ShouldSuppressCA2016(Diagnostic diagnostic, SuppressionAnalysisContext context)
    {
        // Check if the diagnostic location matches a location where our analyzer would report an issue
        var syntaxTree = diagnostic.Location.SourceTree;
        if (syntaxTree == null)
            return false;

        var root = syntaxTree.GetRoot();
        var node = root.FindNode(diagnostic.Location.SourceSpan);

        // Find the invocation expression that contains this diagnostic
        var invocationExpr = node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocationExpr == null)
            return false;

        // Get the semantic model to analyze the invocation
        var semanticModel = context.GetSemanticModel(syntaxTree);
        if (semanticModel == null)
            return false;

        // Check if this is a case our analyzer would handle
        return WouldOurAnalyzerReport(invocationExpr, semanticModel);
    }

    private static bool WouldOurAnalyzerReport(InvocationExpressionSyntax invocationExpr, SemanticModel semanticModel)
    {
        // Check if this invocation has CancellationToken.None arguments
        var cancellationTokenArguments = invocationExpr.GetCancellationTokenArguments(semanticModel);
        if (cancellationTokenArguments.Any())
        {
            var hasTokenNoneArguments = invocationExpr.ArgumentList.Arguments.Any(a =>
                a.IsCancellationTokenNone(semanticModel));
            if (hasTokenNoneArguments)
                return true;
        }

        // Check if this is a method that should have CancellationToken
        if (semanticModel.GetSymbolInfo(invocationExpr).Symbol is IMethodSymbol methodSymbol)
        {
            // Check if method has optional CancellationToken parameter
            bool methodHasOptionalCancellationToken = methodSymbol.Parameters.Any(p => p.Type.IsCancellationToken());
            if (methodHasOptionalCancellationToken)
                return true;

            // Check if method has overload with CancellationToken
            if (methodSymbol.HasOverloadWithCancellationToken())
                return true;
        }

        return false;
    }
}