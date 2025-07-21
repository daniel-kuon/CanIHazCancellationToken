using System.Collections.Generic;
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

namespace CanIHazCancellationTokenAnalyzer.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodInvocationFixProvider))]
[Shared]
public class MethodInvocationFixProvider : CodeFixProvider
{
    private const string PassTokenFromMethodKey = "MethodInvocationFixProvider.PassTokenFromMethod";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [
        Rules.AddCancellationTokenRule.Id, Rules.UseRealCancellationTokenRule.Id,
        Rules.UseOverloadWithCancellationTokenRule.Id
    ];

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
        var invocationExpr = root.FindToken(diagnosticSpan.Start)
           .Parent?.AncestorsAndSelf()
           .OfType<InvocationExpressionSyntax>()
           .FirstOrDefault();
        if (invocationExpr is null) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel is null) return;

        CancellationTokenCandidate[] cancellationTokenCandidates =
            [..GetCancellationTokenCandidates(invocationExpr, semanticModel)];

        foreach (var candidate in cancellationTokenCandidates)
        {
            string key = cancellationTokenCandidates.Length == 1
                             ? PassTokenFromMethodKey
                             : PassTokenFromMethodKey + candidate.Key;
            context.RegisterCodeFix(CodeAction.Create($"Pass {candidate.Description} as CancellationToken",
                    c => AddCancellationTokenArgument(context.Document, invocationExpr, candidate, c),
                    key),
                diagnostic);
        }
    }

    private static IEnumerable<CancellationTokenCandidate> GetCancellationTokenCandidates(
        InvocationExpressionSyntax invocationExpr,
        SemanticModel semanticModel)
    {
        // Find the containing method to look for parameters
        var containingMethod = invocationExpr.GetMethodDeclaration();
        if (containingMethod != null)
            // Look for CancellationToken parameters in the containing method
            foreach (var parameter in containingMethod.ParameterList.Parameters)
            {
                var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);
                if (parameterSymbol?.Type != null && IsCancellationTokenType(parameterSymbol.Type))
                    yield return new CancellationTokenCandidate(CancellationTokenCandidate.CandidateType.Parameter,
                        parameter.Identifier.ValueText);
            }

        // Find the containing class to look for fields and properties
        var containingClass = invocationExpr.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (containingClass != null)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(containingClass);
            if (classSymbol != null)
            {
                // Look for CancellationToken fields
                foreach (var member in classSymbol.GetMembers().OfType<IFieldSymbol>())
                    if (IsCancellationTokenType(member.Type))

                        yield return new CancellationTokenCandidate(CancellationTokenCandidate.CandidateType.Field,
                            member.Name);

                // Look for CancellationToken properties
                foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
                    if (IsCancellationTokenType(member.Type) && member.GetMethod != null &&
                        member.DeclaredAccessibility != Accessibility.Private)
                        yield return new CancellationTokenCandidate(CancellationTokenCandidate.CandidateType.Property,
                            member.Name);
            }
        }
    }

    private static bool IsCancellationTokenType(ITypeSymbol type)
    {
        return type.ToDisplayString() == "System.Threading.CancellationToken";
    }

    private static async Task<Document> AddCancellationTokenArgument(Document document,
        InvocationExpressionSyntax invocationExpr,
        CancellationTokenCandidate? cancellationTokenCandidate,
        CancellationToken cancellationToken)
    {
        var methodDecl = invocationExpr.GetMethodDeclaration();
        if (methodDecl is null) return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

        var paramIdentifier = SyntaxFactory.Identifier(cancellationTokenCandidate?.Identifier ?? "cancellationToken");

        var newMethodDecl = PassTokenToInvokedMethod(invocationExpr, editor, paramIdentifier, methodDecl);

        editor.ReplaceNode(methodDecl, newMethodDecl);

        return editor.GetChangedDocument();
    }

    private static MethodDeclarationSyntax PassTokenToInvokedMethod(InvocationExpressionSyntax invocationExpr,
        DocumentEditor editor,
        SyntaxToken paramIdentifier,
        MethodDeclarationSyntax methodDecl)
    {
        var argumentList = invocationExpr.ArgumentList;
        var cancellationTokenNoneArgument =
            argumentList.Arguments.FirstOrDefault(a => a.IsCancellationTokenNone(editor.SemanticModel));
        var newArgument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(paramIdentifier));
        var newArgumentList = cancellationTokenNoneArgument == null
                                  ? argumentList.AddArguments(newArgument)
                                  : argumentList.ReplaceNode(cancellationTokenNoneArgument, newArgument);

        var newInvocationExpr = invocationExpr.WithArgumentList(newArgumentList);
        methodDecl = methodDecl.ReplaceNode(invocationExpr, newInvocationExpr);
        return methodDecl;
    }
}
