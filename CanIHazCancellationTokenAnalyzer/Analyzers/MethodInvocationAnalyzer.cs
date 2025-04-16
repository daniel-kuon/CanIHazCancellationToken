using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CanIHazCancellationTokenAnalyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MethodInvocationAnalyzer : DiagnosticAnalyzer
{
    public const string AddCancellationTokenDiagnosticId = "CHIHC002";
    public const string UseRealCancellationTokenDiagnosticId = "CHIHC003";

    private static readonly DiagnosticDescriptor AddCancellationTokenRule = new(AddCancellationTokenDiagnosticId,
        "CancellationToken is missing in method call",
        "Method call should pass a CancellationToken",
        "Usage",
        DiagnosticSeverity.Warning,
        true);

    private static readonly DiagnosticDescriptor UseRealCancellationTokenRule = new(
        UseRealCancellationTokenDiagnosticId,
        "Use real CancellationToken instead of CancellationToken.None",
        "Method call should pass a real CancellationToken instead of CancellationToken.None",
        "Usage",
        DiagnosticSeverity.Info,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        AddCancellationTokenRule,
        UseRealCancellationTokenRule
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
                    a.IsCancellationTokenNone(context.SemanticModel))
               .ToList();

            foreach (var argument in argumentsWithCancellationToken)
            {
                var diagnostic = Diagnostic.Create(UseRealCancellationTokenRule, argument.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        else if (invocationExpr.GetOverloadWithCancellationToken(context.SemanticModel).Any())
        {
            var diagnostic = Diagnostic.Create(AddCancellationTokenRule, invocationExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}

public static class CancellationTokenExtensions
{
    public static List<ArgumentSyntax> GetCancellationTokenArguments(this InvocationExpressionSyntax invocationExpr,
        SemanticModel semanticModel)
    {
        return invocationExpr.ArgumentList.Arguments
           .Where(a => semanticModel.GetTypeInfo(a.Expression).Type.IsCancellationToken())
           .ToList();
    }

    public static bool IsCancellationTokenNone(this ArgumentSyntax argument, SemanticModel semanticModel)
    {
        return argument.Expression is MemberAccessExpressionSyntax memberAccess &&
               memberAccess.Name.Identifier.ValueText == nameof(CancellationToken.None) &&
               (semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol as INamedTypeSymbol)?.ToString() ==
               "System.Threading.CancellationToken";
    }

    public static bool IsCancellationToken(this ITypeSymbol? typeSymbol)
    {
        return typeSymbol?.ToDisplayString() == "System.Threading.CancellationToken";
    }

    public static MethodDeclarationSyntax? GetMethodDeclaration(this InvocationExpressionSyntax invocationExpr)
    {
        return invocationExpr.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
    }

    public static IEnumerable<ParameterSyntax> GetCancellationTokenParameters(this MethodDeclarationSyntax methodDecl,
        SemanticModel semanticModel)
    {
        return methodDecl.ParameterList.Parameters.Where(p =>
            semanticModel.GetTypeInfo(p.Type!).Type.IsCancellationToken());
    }

    public static IEnumerable<PropertyDeclarationSyntax> GetCancellationTokenProperties(
        this ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel)
    {
        return classDecl.Members.OfType<PropertyDeclarationSyntax>()
           .Where(p => semanticModel.GetTypeInfo(p.Type).Type.IsCancellationToken());
    }

    public static IEnumerable<FieldDeclarationSyntax> GetCancellationTokenFields(this ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel)
    {
        return classDecl.Members.OfType<FieldDeclarationSyntax>()
           .Where(f => semanticModel.GetTypeInfo(f.Declaration.Type).Type.IsCancellationToken());
    }

    public static IMethodSymbol[] GetOverloadWithCancellationToken(this InvocationExpressionSyntax invocationExpr,
        SemanticModel semanticModel)
    {
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpr).Symbol as IMethodSymbol;
        if (methodSymbol is null) return [];

        bool methodHasOptionalCancellationToken =
            methodSymbol.Parameters.Any(p => p.IsOptional && p.Type.IsCancellationToken());
        if (methodHasOptionalCancellationToken) return [methodSymbol];

        var overloadsWithCancellationToken = methodSymbol.ContainingType.GetMembers()
           .OfType<IMethodSymbol>()
           .Where(m => m.Name == methodSymbol.Name && m.Parameters.Any(p => p.Type.IsCancellationToken()) &&
                       m.Parameters.Length == methodSymbol.Parameters.Length + 1 &&
                       MethodHasSameParametersAsOriginal(m, methodSymbol));

        return overloadsWithCancellationToken.ToArray();
    }

    private static bool MethodHasSameParametersAsOriginal(IMethodSymbol m, IMethodSymbol methodSymbol)
    {
        return m.Parameters.Take(methodSymbol.Parameters.Length)
           .All(p => p.Type.ToDisplayString() == methodSymbol.Parameters[p.Ordinal].Type.ToDisplayString());
    }
}
