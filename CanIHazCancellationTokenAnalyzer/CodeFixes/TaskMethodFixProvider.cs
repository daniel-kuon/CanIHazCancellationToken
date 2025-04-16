using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CanIHazCancelationTokenAnalyzer.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TaskMethodFixProvider)), Shared]
    public class TaskMethodFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ["CHIHC001"];

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            if (root is null) return;

            var diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic is null) return;

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var methodDecl = root.FindToken(diagnosticSpan.Start).Parent
                ?.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDecl is null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add optional CancellationToken",
                    c => AddCancellationTokenParameter(context.Document, methodDecl, c),
                    "Add optional CancellationToken"),
                diagnostic);
        }

        private async Task<Document> AddCancellationTokenParameter(
            Document document,
            MethodDeclarationSyntax methodDecl,
            CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            var newParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(SyntaxFactory.ParseTypeName("System.Threading.CancellationToken"))
                .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("default")));

            var newParamList = methodDecl.ParameterList.AddParameters(newParameter);
            var newMethodDecl = methodDecl.WithParameterList(newParamList);

            editor.ReplaceNode(methodDecl, newMethodDecl);
            return editor.GetChangedDocument();
        }
    }
}
