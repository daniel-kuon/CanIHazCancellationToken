using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Linq;

namespace CanIHazCancellationTokenAnalyzer.Helpers;

/// <summary>
/// Helper class for managing using statements and type names
/// </summary>
public static class UsingStatementHelper
{
    /// <summary>
    /// Gets the appropriate type name syntax based on configuration
    /// </summary>
    /// <param name="fullyQualifiedTypeName">The fully qualified type name (e.g., "System.Threading.CancellationToken")</param>
    /// <param name="preferUsingStatements">Whether to prefer short names with using statements</param>
    /// <returns>The appropriate type name syntax</returns>
    public static TypeSyntax GetTypeName(string fullyQualifiedTypeName, bool preferUsingStatements)
    {
        if (!preferUsingStatements)
        {
            return SyntaxFactory.ParseTypeName(fullyQualifiedTypeName);
        }
        
        // Use short name - extract the type name after the last dot
        var lastDotIndex = fullyQualifiedTypeName.LastIndexOf('.');
        var shortName = lastDotIndex >= 0 
            ? fullyQualifiedTypeName.Substring(lastDotIndex + 1) 
            : fullyQualifiedTypeName;
            
        return SyntaxFactory.ParseTypeName(shortName);
    }
    
    /// <summary>
    /// Gets the appropriate expression syntax for accessing a static member based on configuration
    /// </summary>
    /// <param name="fullyQualifiedExpression">The fully qualified expression (e.g., "System.Threading.CancellationToken.None")</param>
    /// <param name="preferUsingStatements">Whether to prefer short names with using statements</param>
    /// <returns>The appropriate expression syntax</returns>
    public static ExpressionSyntax GetStaticMemberAccess(string fullyQualifiedExpression, bool preferUsingStatements)
    {
        if (!preferUsingStatements)
        {
            return SyntaxFactory.ParseExpression(fullyQualifiedExpression);
        }
        
        // Extract short form - find the class name and member name
        var parts = fullyQualifiedExpression.Split('.');
        if (parts.Length < 2)
        {
            return SyntaxFactory.ParseExpression(fullyQualifiedExpression);
        }
        
        var className = parts[parts.Length - 2]; // Second to last part (class name)
        var memberName = parts[parts.Length - 1]; // Last part (member name)
        
        return SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(className),
            SyntaxFactory.IdentifierName(memberName));
    }
    
    /// <summary>
    /// Adds a using directive if it doesn't already exist
    /// </summary>
    /// <param name="root">The compilation unit root</param>
    /// <param name="namespaceName">The namespace to add (e.g., "System.Threading")</param>
    /// <returns>The updated compilation unit with the using directive added</returns>
    public static CompilationUnitSyntax EnsureUsingDirective(CompilationUnitSyntax root, string namespaceName)
    {
        // Check if the using directive already exists
        var existingUsing = root.Usings.FirstOrDefault(u => 
            u.Name?.ToString() == namespaceName);
            
        if (existingUsing == null)
        {
            var usingDirective = SyntaxFactory.UsingDirective(
                SyntaxFactory.ParseName(namespaceName));
            return root.AddUsings(usingDirective);
        }
        
        return root;
    }
    
    /// <summary>
    /// Extracts the namespace from a fully qualified type name
    /// </summary>
    /// <param name="fullyQualifiedTypeName">The fully qualified type name</param>
    /// <returns>The namespace, or null if no namespace found</returns>
    public static string? GetNamespaceFromType(string fullyQualifiedTypeName)
    {
        var lastDotIndex = fullyQualifiedTypeName.LastIndexOf('.');
        return lastDotIndex >= 0 ? fullyQualifiedTypeName.Substring(0, lastDotIndex) : null;
    }
}