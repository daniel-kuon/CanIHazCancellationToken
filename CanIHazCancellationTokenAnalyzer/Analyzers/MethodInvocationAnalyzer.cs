using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CanIHazCancellationTokenAnalyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MethodInvocationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        Rules.AddCancellationTokenRule,
        Rules.UseRealCancellationTokenRule,
        Rules.UseOverloadWithCancellationTokenRule
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        var cancellationTokenArguments = invocationExpr.GetCancellationTokenArguments(context.SemanticModel);

        if (cancellationTokenArguments.Any())
        {
            var argumentsWithCancellationToken = invocationExpr.ArgumentList.Arguments.Where(a =>
                a.IsCancellationTokenNone(context.SemanticModel));

            foreach (var argument in argumentsWithCancellationToken)
            {
                var diagnostic = Diagnostic.Create(Rules.UseRealCancellationTokenRule, argument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        else if (context.SemanticModel.GetSymbolInfo(invocationExpr).Symbol is IMethodSymbol methodSymbol)
        {
            bool methodHasOptionalCancellationToken = methodSymbol.Parameters.Any(p => p.Type.IsCancellationToken());
            if (methodHasOptionalCancellationToken)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rules.AddCancellationTokenRule, invocationExpr.GetLocation()));
            }
            else if (methodSymbol.HasOverloadWithCancellationToken())
            {
                var diagnostic = Diagnostic.Create(Rules.UseOverloadWithCancellationTokenRule,
                    invocationExpr.GetLocation(),
                    ImmutableDictionary.Create<string, string?>().Add("MethodName", methodSymbol.Name));
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
