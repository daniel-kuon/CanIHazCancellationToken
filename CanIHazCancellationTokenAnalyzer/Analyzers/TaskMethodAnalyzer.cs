using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CanIHazCancellationTokenAnalyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TaskMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CHIHC001";
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "CancellationToken is missing",
            "Method returning Task/ValueTask should include an optional CancellationToken parameter",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDecl = (MethodDeclarationSyntax)context.Node;

            var returnTypeSymbol =
                context.SemanticModel.GetSymbolInfo(methodDecl.ReturnType).Symbol as INamedTypeSymbol;
            if (returnTypeSymbol is null) return;

            var fullName = returnTypeSymbol.ToDisplayString();
            if (!fullName.StartsWith("System.Threading.Tasks.Task")
                && !fullName.StartsWith("System.Threading.Tasks.ValueTask")
                && !(methodDecl.Modifiers.Any(SyntaxKind.AsyncKeyword) && fullName == "void"))
            {
                return;
            }

            var hasCancellationTokenParam = methodDecl.ParameterList?.Parameters
                .Any(p => context.SemanticModel.GetTypeInfo(p.Type!).Type?.ToDisplayString()
                    == "System.Threading.CancellationToken") == true;

            if (!hasCancellationTokenParam)
            {
                var diagnostic = Diagnostic.Create(Rule, methodDecl.Identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
