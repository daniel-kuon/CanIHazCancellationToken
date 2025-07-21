using CanIHazCancellationTokenAnalyzer.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CanIHazCancellationTokenAnalyzer.Tests.Configuration;

/// <summary>
/// Tests for CodeFixConfiguration
/// </summary>
public class CodeFixConfigurationTests
{
    /// <summary>
    /// Test implementation of AnalyzerConfigOptions for testing
    /// </summary>
    private class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _options;

        public TestAnalyzerConfigOptions(Dictionary<string, string> options)
        {
            _options = options ?? new Dictionary<string, string>();
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _options.TryGetValue(key, out value);
        }
    }

    /// <summary>
    /// Tests that GetPreferUsingStatements returns false by default
    /// </summary>
    public void TestGetPreferUsingStatements_DefaultsFalse()
    {
        var options = new TestAnalyzerConfigOptions(new Dictionary<string, string>());
        var result = CodeFixConfiguration.GetPreferUsingStatements(options);
        
        if (result != false)
        {
            throw new System.Exception("Expected default value to be false");
        }
    }

    /// <summary>
    /// Tests that GetPreferUsingStatements returns true when configured
    /// </summary>
    public void TestGetPreferUsingStatements_ReturnsConfiguredValue()
    {
        var options = new TestAnalyzerConfigOptions(new Dictionary<string, string>
        {
            { CodeFixConfiguration.PreferUsingStatementsKey, "true" }
        });
        var result = CodeFixConfiguration.GetPreferUsingStatements(options);
        
        if (result != true)
        {
            throw new System.Exception("Expected configured value to be true");
        }
    }

    /// <summary>
    /// Tests that GetPreferUsingStatements handles invalid values gracefully
    /// </summary>
    public void TestGetPreferUsingStatements_HandlesInvalidValues()
    {
        var options = new TestAnalyzerConfigOptions(new Dictionary<string, string>
        {
            { CodeFixConfiguration.PreferUsingStatementsKey, "invalid" }
        });
        var result = CodeFixConfiguration.GetPreferUsingStatements(options);
        
        if (result != false)
        {
            throw new System.Exception("Expected invalid value to default to false");
        }
    }
}