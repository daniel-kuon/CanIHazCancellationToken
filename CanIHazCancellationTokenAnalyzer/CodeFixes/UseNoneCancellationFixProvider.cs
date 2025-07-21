using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanIHazCancellationTokenAnalyzer.Analyzers;
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

            var newArgument = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("System.Threading.CancellationToken.None"));
            var newArgumentList = invocationExpr.ArgumentList.AddArguments(newArgument);

            var newInvocationExpr = invocationExpr.WithArgumentList(newArgumentList);
            editor.ReplaceNode(invocationExpr, newInvocationExpr);

            return editor.GetChangedDocument();
        }
    }
}
