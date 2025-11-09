using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanIHazCancellationTokenAnalyzer.Analyzers;
using CanIHazCancellationTokenAnalyzer.Configuration;
using CanIHazCancellationTokenAnalyzer.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CanIHazCancellationTokenAnalyzer.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNoneCancellationTokenFixProvider)), Shared]
    public class UseNoneCancellationTokenFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = [Rules.AddCancellationTokenRule.Id];

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            if (root is null) return;

            var diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic is null) return;

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var invocationExpr = root.FindToken(diagnosticSpan.Start)
               .Parent?.AncestorsAndSelf()
               .OfType<InvocationExpressionSyntax>()
               .FirstOrDefault();
            if (invocationExpr is null) return;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            if (semanticModel is null) return;

                context.RegisterCodeFix(CodeAction.Create("Pass CancellationToken.None to express intent",
                        c => AddCancellationTokenArgument(context.Document,
                            invocationExpr,
                            c),
                        "Pass CancellationToken.None to express intent",CodeActionPriority.Low),
                    diagnostic);
        }

        private async Task<Document> AddCancellationTokenArgument(Document document,
            InvocationExpressionSyntax invocationExpr,
            CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
            
            // Get configuration options from the document
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
            var options = syntaxTree != null 
                ? document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree)
                : null;
            var preferUsingStatements = options != null && CodeFixConfiguration.GetPreferUsingStatements(options);
            
            var expression = UsingStatementHelper.GetStaticMemberAccess("System.Threading.CancellationToken.None", preferUsingStatements);
            var newArgument = SyntaxFactory.Argument(expression);
            var newArgumentList = invocationExpr.ArgumentList.AddArguments(newArgument);

            var newInvocationExpr = invocationExpr.WithArgumentList(newArgumentList);
            editor.ReplaceNode(invocationExpr, newInvocationExpr);

            // Add using directive if needed
            if (preferUsingStatements)
            {
                var root = await editor.GetChangedDocument().GetSyntaxRootAsync(cancellationToken) as CompilationUnitSyntax;
                var cancellationTokenNamespace = UsingStatementHelper.GetNamespaceFromType("System.Threading.CancellationToken");
                if (root != null && cancellationTokenNamespace != null)
                {
                    var newRoot = UsingStatementHelper.EnsureUsingDirective(root, cancellationTokenNamespace);
                    return document.WithSyntaxRoot(newRoot);
                }
            }
            
            return editor.GetChangedDocument();
        }
    }
}
