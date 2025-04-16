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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodInvocationFixProvider)), Shared]
    public class MethodInvocationFixProvider : CodeFixProvider
    {
        public const string CreateParameterAndUseItKey = "MethodInvocationFixProvider.CreateParameterAndUseIt";
        public const string PassTokenFromMethodKey = "MethodInvocationFixProvider.PassTokenFromMethod";

        public override ImmutableArray<string> FixableDiagnosticIds =>
        [
            MethodInvocationAnalyzer.AddCancellationTokenDiagnosticId,
            MethodInvocationAnalyzer.UseRealCancellationTokenDiagnosticId
        ];

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

            var cancellationTokenParameters = invocationExpr.GetMethodDeclaration()
                                                ?.GetCancellationTokenParameters(semanticModel)
                                                 .ToList() ?? [];

            if (cancellationTokenParameters.Count == 1)
            {
                context.RegisterCodeFix(CodeAction.Create("Pass existing CancellationToken from method",
                        c => AddCancellationTokenArgument(context.Document,
                            invocationExpr,
                            cancellationTokenParameters.First(),
                            c),
                        PassTokenFromMethodKey),
                    diagnostic);
            }
            else if (cancellationTokenParameters.Count > 1)
            {
                foreach (var cancellationTokenParam in cancellationTokenParameters)
                {
                    context.RegisterCodeFix(CodeAction.Create(
                            $"Pass parameter '{cancellationTokenParam.Identifier}' as CancellationToken",
                            c => AddCancellationTokenArgument(context.Document,
                                invocationExpr,
                                cancellationTokenParam,
                                c),
                            PassTokenFromMethodKey),
                        diagnostic);
                }
            }
            else
            {
                context.RegisterCodeFix(CodeAction.Create("Add optional CancellationToken parameter and use it",
                        cancellationToken =>
                            AddCancellationTokenArgument(context.Document, invocationExpr, null, cancellationToken),
                        CreateParameterAndUseItKey),
                    diagnostic);
            }
        }

        private async Task<Document> AddCancellationTokenArgument(Document document,
            InvocationExpressionSyntax invocationExpr,
            ParameterSyntax? cancellationTokenParam,
            CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            var methodDecl = invocationExpr.GetMethodDeclaration();
            if (methodDecl is null) return document;

            var originalMethodDecl = methodDecl;

            var paramIdentifier = cancellationTokenParam?.Identifier ?? SyntaxFactory.Identifier("cancellationToken");

            var argumentList = invocationExpr.ArgumentList;
            var cancellationTokenNoneArgument =
                argumentList.Arguments.FirstOrDefault(a => a.IsCancellationTokenNone(editor.SemanticModel));
            var newArgument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(paramIdentifier));
            var newArgumentList = cancellationTokenNoneArgument == null
                                      ? argumentList.AddArguments(newArgument)
                                      : argumentList.ReplaceNode(cancellationTokenNoneArgument, newArgument);

            var newInvocationExpr = invocationExpr.WithArgumentList(newArgumentList);
            methodDecl = methodDecl.ReplaceNode(invocationExpr, newInvocationExpr);

            if (cancellationTokenParam is null)
            {
                cancellationTokenParam = SyntaxFactory.Parameter(paramIdentifier)
                   .WithType(SyntaxFactory.ParseTypeName("System.Threading.CancellationToken"))
                   .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("default")));


                var newParamList = methodDecl.ParameterList.AddParameters(cancellationTokenParam);
                methodDecl = methodDecl.WithParameterList(newParamList);
            }

            editor.ReplaceNode(originalMethodDecl, methodDecl);

            return editor.GetChangedDocument();
        }
    }
}
