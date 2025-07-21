using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace CanIHazCancellationTokenAnalyzer;

public static class CancellationTokenExtensions
{
    public static IEnumerable<ArgumentSyntax> GetCancellationTokenArguments(
        this InvocationExpressionSyntax invocationExpr,
        SemanticModel semanticModel)
    {
        return invocationExpr.ArgumentList.Arguments.Where(a =>
            semanticModel.GetTypeInfo(a.Expression).Type.IsCancellationToken());
    }

    public static bool IsCancellationTokenNone(this ArgumentSyntax argument, SemanticModel semanticModel)
    {
        return argument.Expression is MemberAccessExpressionSyntax
               {
                   Name.Identifier.ValueText: nameof(CancellationToken.None)
               } memberAccess &&
               (CSharpExtensions.GetSymbolInfo(semanticModel, memberAccess.Expression).Symbol as INamedTypeSymbol)?.ToString() ==
               "System.Threading.CancellationToken";
    }

    public static bool IsCancellationToken(this ITypeSymbol? typeSymbol) =>
        typeSymbol?.ToDisplayString() == "System.Threading.CancellationToken";

    public static MethodDeclarationSyntax? GetMethodDeclaration(this InvocationExpressionSyntax invocationExpr) =>
        invocationExpr.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();

    public static IEnumerable<ParameterSyntax> GetCancellationTokenParameters(this MethodDeclarationSyntax methodDecl,
        SemanticModel semanticModel) =>
        methodDecl.ParameterList.Parameters.Where(p => CSharpExtensions.GetTypeInfo(semanticModel, p.Type!).Type.IsCancellationToken());

    public static IEnumerable<PropertyDeclarationSyntax> GetCancellationTokenProperties(
        this ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel) =>
        classDecl.Members.OfType<PropertyDeclarationSyntax>()
           .Where(p => CSharpExtensions.GetTypeInfo(semanticModel, p.Type).Type.IsCancellationToken());

    public static IEnumerable<FieldDeclarationSyntax> GetCancellationTokenFields(this ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel) =>
        classDecl.Members.OfType<FieldDeclarationSyntax>()
           .Where(f => CSharpExtensions.GetTypeInfo(semanticModel, f.Declaration.Type).Type.IsCancellationToken());

    public static bool HasOverloadWithCancellationToken(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.ContainingType.GetMembers()
           .OfType<IMethodSymbol>()
           .Any(m => m.Name == methodSymbol.Name && m.Parameters.Any(p => p.Type.IsCancellationToken()) &&
                     m.Parameters.Length == methodSymbol.Parameters.Length + 1 &&
                     m.HasSameParametersAsOriginal(methodSymbol));
    }

    private static bool HasSameParametersAsOriginal(this IMethodSymbol m, IMethodSymbol methodSymbol)
    {
        return m.Parameters.Take(methodSymbol.Parameters.Length)
           .All(p => SymbolEqualityComparer.Default.Equals(p.Type.OriginalDefinition,
                methodSymbol.Parameters[p.Ordinal].Type.OriginalDefinition));
    }
}
