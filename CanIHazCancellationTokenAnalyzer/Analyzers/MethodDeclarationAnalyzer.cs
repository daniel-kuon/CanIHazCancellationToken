using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using CanIHazCancellationTokenAnalyzer.Analyzers;

namespace CanIHazCancellationTokenAnalyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MethodDeclarationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rules.TaskRule, Rules.AsyncVoidRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }


    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        INamedTypeSymbol GetTypeByMetadataName(string name)
        {
            return context.Compilation.GetTypeByMetadataName(name) ??
                   throw new InvalidOperationException(
                       $"{name} not found. Ensure the correct assemblies are referenced.");
        }

        // Cache common symbols for performance
        var taskSymbol = GetTypeByMetadataName("System.Threading.Tasks.Task");
        var genericTaskSymbol = GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var valueTaskSymbol = GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var genericValueTaskSymbol = GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

        var cancellationTokenSymbol = GetTypeByMetadataName("System.Threading.CancellationToken");

        var analyzer =
            new CachedSymbolAnalyzer([taskSymbol, valueTaskSymbol, genericTaskSymbol, genericValueTaskSymbol],
                cancellationTokenSymbol);
        context.RegisterSyntaxNodeAction(analyzer.AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private class CachedSymbolAnalyzer(INamedTypeSymbol[] taskSymbols, INamedTypeSymbol cancellationTokenSymbol)
    {
        public void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDecl = (MethodDeclarationSyntax)context.Node;

            if (context.SemanticModel.GetSymbolInfo(methodDecl.ReturnType).Symbol is not INamedTypeSymbol
                returnTypeSymbol)
                return;

            if (MethodIsCompliant(context, returnTypeSymbol, methodDecl) || CannotAddParameter(context, methodDecl))
                return;

            DiagnosticDescriptor ruleToUse = IsTaskLikeType(returnTypeSymbol) ? Rules.TaskRule : Rules.AsyncVoidRule;
            var diagnostic = Diagnostic.Create(ruleToUse, methodDecl.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private bool MethodIsCompliant(SyntaxNodeAnalysisContext context,
            INamedTypeSymbol returnTypeSymbol,
            MethodDeclarationSyntax methodDecl)
        {
            var isTaskLike = IsTaskLikeType(returnTypeSymbol);
            var isAsyncVoid = IsAsyncVoidMethod(methodDecl, returnTypeSymbol);

            if (!isTaskLike && !isAsyncVoid)
                return true;

            bool hasCancellationTokenParam = methodDecl.ParameterList.Parameters.Any(p =>
                IsCancellationTokenType(context.SemanticModel.GetTypeInfo(p.Type!).Type));

            if (hasCancellationTokenParam)
                return true;
            return false;
        }

        private static bool CannotAddParameter(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDecl)
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl);

            if (methodSymbol is null)
                return true;

            if (MethodImplementsInterface(methodSymbol) || methodDecl.Modifiers.Any(SyntaxKind.OverrideKeyword))
                return true;

            if (methodSymbol.HasOverloadWithCancellationToken())
                return true;
            return false;
        }

        private static bool MethodImplementsInterface(IMethodSymbol methodSymbol) =>
            methodSymbol.ContainingType.AllInterfaces.SelectMany(interfaceType => interfaceType.GetMembers())
               .OfType<IMethodSymbol>()
               .Select(methodSymbol.ContainingType.FindImplementationForInterfaceMember)
               .Any(implementation => SymbolEqualityComparer.Default.Equals(implementation, methodSymbol));

        private bool IsTaskLikeType(INamedTypeSymbol typeSymbol) =>
            taskSymbols.Contains(typeSymbol.OriginalDefinition, SymbolEqualityComparer.Default);

        private bool IsCancellationTokenType(ITypeSymbol? typeSymbol) =>
            SymbolEqualityComparer.Default.Equals(typeSymbol, cancellationTokenSymbol);

        private static bool IsAsyncVoidMethod(MethodDeclarationSyntax methodDecl, INamedTypeSymbol returnTypeSymbol) =>
            methodDecl.Modifiers.Any(SyntaxKind.AsyncKeyword) &&
            returnTypeSymbol.SpecialType == SpecialType.System_Void;
    }
}
