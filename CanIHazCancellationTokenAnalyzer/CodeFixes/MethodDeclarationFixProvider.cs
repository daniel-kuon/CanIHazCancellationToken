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

namespace CanIHazCancelationTokenAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodDeclarationFixProvider))]
[Shared]
public class MethodDeclarationFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [Rules.TaskRule.Id, Rules.AsyncVoidRule.Id];

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var methodDecl = root.FindToken(diagnosticSpan.Start)
           .Parent?.AncestorsAndSelf()
           .OfType<MethodDeclarationSyntax>()
           .FirstOrDefault();
        if (methodDecl is null) return;

        context.RegisterCodeFix(CodeAction.Create("Add optional CancellationToken",
                c => AddCancellationTokenParameter(context.Document, methodDecl, c),
                "Add optional CancellationToken"),
            diagnostic);
    }

    private static async Task<Document> AddCancellationTokenParameter(Document document,
        MethodDeclarationSyntax methodDecl,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        
        // Get configuration options from the document
        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken);
        var options = syntaxTree != null 
            ? document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree)
            : null;
        var preferUsingStatements = options != null && CodeFixConfiguration.GetPreferUsingStatements(options);
        
        var typeName = UsingStatementHelper.GetTypeName("System.Threading.CancellationToken", preferUsingStatements);
        
        var newParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
           .WithType(typeName)
           .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression("default")));

        var newParamList = methodDecl.ParameterList.AddParameters(newParameter);
        var newMethodDecl = methodDecl.WithParameterList(newParamList);

        editor.ReplaceNode(methodDecl, newMethodDecl);
        
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
