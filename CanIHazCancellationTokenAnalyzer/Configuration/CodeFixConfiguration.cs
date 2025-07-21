using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CanIHazCancellationTokenAnalyzer.Configuration;

/// <summary>
/// Configuration options for code fix providers
/// </summary>
public class CodeFixConfiguration
{
    /// <summary>
    /// Configuration key for preferring using statements over fully qualified names
    /// </summary>
    public const string PreferUsingStatementsKey = "CanIHazCancellationToken.PreferUsingStatements";
    
    /// <summary>
    /// Gets whether to prefer adding using statements and using short names instead of fully qualified names
    /// </summary>
    /// <param name="analyzerConfigOptions">The analyzer config options</param>
    /// <returns>True if using statements should be preferred; false to use fully qualified names (default)</returns>
    public static bool GetPreferUsingStatements(AnalyzerConfigOptions analyzerConfigOptions)
    {
        return analyzerConfigOptions.TryGetValue(PreferUsingStatementsKey, out var value) 
               && bool.TryParse(value, out var boolValue) 
               && boolValue;
    }
}