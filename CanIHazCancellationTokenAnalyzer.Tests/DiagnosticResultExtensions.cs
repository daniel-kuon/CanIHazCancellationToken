using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

public static class DiagnosticResultExtensions
{
    public static DiagnosticResult WithSpan(this DiagnosticResult result, string source, string match)
    {
        return result.WithSpans(source, match).Single();
    }

    public static IEnumerable<DiagnosticResult> WithSpans(this DiagnosticResult result, string source, string pattern)
    {
        if (pattern.Contains('\n'))
        {
            throw new ArgumentException("Pattern string must not contain newline characters");
        }

        var regex = new Regex(Regex.Escape(pattern));
        var match = regex.Match(source);

        if (!match.Success)
        {
            throw new ArgumentException("Match string must occur at least once in source");
        }

        while (match.Success)
        {
            var startLine = source[..match.Index].Count(c => c == '\n') + 1;
            var positionInLine = source.Split('\n')[startLine - 1].IndexOf(pattern, StringComparison.Ordinal) + 1;

            yield return result.WithSpan(startLine, positionInLine, startLine, positionInLine + pattern.Length);

            match = match.NextMatch();
        }
    }

    public static DiagnosticResult WithSpan(this DiagnosticResult result, (int Line, int StartPosition, int Length) span)
    {
        return result.WithSpan(span.Line, span.StartPosition, span.Line, span.StartPosition + span.Length);
    }
}
