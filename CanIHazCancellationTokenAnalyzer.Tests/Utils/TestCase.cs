using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CanIHazCancellationTokenAnalyzer.Analyzers;

namespace CanIHazCancellationTokenAnalyzer.Tests.Utils;

public partial class TestCase
{
    public TestCase(ReturnType returnType, Method methodConfiguration,  params IEnumerable<ParameterType> parameterTypes) : this(returnType, methodConfiguration,
        CancellationTokenUsage.None, parameterTypes)
    {
    }

    public TestCase(ReturnType returnType, Method methodConfiguration, CancellationTokenUsage cancellationTokenUsage,  params IEnumerable<ParameterType> parameterTypes)
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes.ToArray();
        IsAsync = methodConfiguration.HasFlag(Method.IsAsync) || returnType == ReturnType.Void;
        CancellationTokenUsage = cancellationTokenUsage;

        if ( cancellationTokenUsage == CancellationTokenUsage.PassesCancellationToken &&
            !ParameterTypes.Contains(ParameterType.CancellationToken) &&
            !ParameterTypes.Contains(ParameterType.CancellationTokenWithDefault))
        {
            throw new ArgumentException(
                "A CancellationToken parameter is required when PassesCancellationToken is true");
        }
    }

    public string DeclarationAnalyzerRuleId => ReturnType switch
    {
        ReturnType.Task => Rules.TaskRule.Id,
        ReturnType.GenericTask => Rules.TaskRule.Id,
        ReturnType.ValueTask => Rules.TaskRule.Id,
        ReturnType.GenericValueTask => Rules.TaskRule.Id,
        ReturnType.Void when IsAsync  => Rules.AsyncVoidRule.Id,
        _ => throw new ArgumentOutOfRangeException(nameof(ReturnType), ReturnType, null)
    };

    public string InvocationAnalyzerRuleId => CancellationTokenUsage switch
    {
        CancellationTokenUsage.None => Rules.UseOverloadWithCancellationTokenRule.Id,
        CancellationTokenUsage.UsesDefaultCancellationToken => Rules.UseRealCancellationTokenRule.Id,
        CancellationTokenUsage.UsesNoneCancellationToken => Rules.UseRealCancellationTokenRule.Id,
        CancellationTokenUsage.PassesCancellationToken => throw new ArgumentException(
            "InvocationAnalyzerRuleId is not applicable when PassesCancellationToken is true"),
        _ => throw new ArgumentOutOfRangeException(nameof(ReturnType), ReturnType, null)
    };

    private CancellationTokenUsage CancellationTokenUsage { get; }

    private bool IsAsync { get; }

    private ParameterType[] ParameterTypes { get; }

    private ReturnType ReturnType { get; }

    private Method MethodConfiguration
    {
        get
        {
            var method = Method.None;
            if (IsAsync)
            {
                method |= Method.IsAsync;
            }

            return method;
        }
    }

    public TestCase WithPassesCancellationToken()
    {
        var methodConfiguration = MethodConfiguration;
        var parameterTypes = ParameterTypes.ToList();

        if (!parameterTypes.Contains(ParameterType.CancellationToken) &&
            !parameterTypes.Contains(ParameterType.CancellationTokenWithDefault))
        {
            parameterTypes.Add(ParameterType.CancellationTokenWithDefault);
        }

        return new TestCase(ReturnType, methodConfiguration, CancellationTokenUsage.PassesCancellationToken, parameterTypes);
    }

    public TestCase WithOptionalCancellationToken()
    {
        var parameterTypes = ParameterTypes.ToList();
        if (!parameterTypes.Contains(ParameterType.CancellationToken) &&
            !parameterTypes.Contains(ParameterType.CancellationTokenWithDefault))
        {
            parameterTypes.Add(ParameterType.CancellationTokenWithDefault);
        }

        return new TestCase(ReturnType, MethodConfiguration, parameterTypes);
    }


    public override string ToString()
    {
        //language=cs
        return $$"""
                 using System.Threading.Tasks;
                 using System.Threading;

                 class Program
                 {
                     public {{(IsAsync ? "async " : "")}}{{BuildReturnType()}} TestSubjectMethod({{BuildParameters()}})
                     {
                         {{BuildBody()}}
                     }
                 }
                 """;
    }

    public string Code => ToString();

    private string BuildParameters()
    {
        return string.Join(", ", ParameterTypes.Select(p => p switch
        {
            ParameterType.OtherParam => "int param",
            ParameterType.OtherParamWithDefault => "int param = 42",
            ParameterType.CancellationToken => "System.Threading.CancellationToken cancellationToken",
            ParameterType.CancellationTokenWithDefault => "System.Threading.CancellationToken cancellationToken = default",
            _ => throw new ArgumentOutOfRangeException()
        }));
    }

    private string BuildBody()
    {
        var cancellationToken = CancellationTokenUsage switch
        {
            CancellationTokenUsage.PassesCancellationToken => ", cancellationToken",
            CancellationTokenUsage.UsesDefaultCancellationToken => ", default",
            CancellationTokenUsage.UsesNoneCancellationToken => ", System.Threading.CancellationToken.None",
            CancellationTokenUsage.None => "",
            _ => throw new ArgumentOutOfRangeException(nameof(CancellationTokenUsage), CancellationTokenUsage, null)
        };
        return ReturnType switch
        {
            ReturnType.Task => IsAsync
                ? $"await Task.Delay(100{cancellationToken});"
                : $"return Task.Delay(100{cancellationToken});",
            ReturnType.ValueTask => IsAsync
                ? $"await Task.Delay(100{cancellationToken});"
                : $"return new ValueTask(Task.Delay(100{cancellationToken}));",
            ReturnType.GenericTask => IsAsync
                ? $"await Task.Delay(100{cancellationToken});\n        return 42;"
                : $"return Task.Run(() => 42{cancellationToken});",
            ReturnType.GenericValueTask => IsAsync
                ? $"await Task.Delay(100{cancellationToken});\n        return 42;"
                : $"return new ValueTask<int>(Task.Run(() => 42{cancellationToken}));",
            ReturnType.Void => IsAsync
                ? $"await Task.Delay(100{cancellationToken});"
                : "",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string BuildReturnType()
    {
        return ReturnType switch
        {
            ReturnType.Task => "Task",
            ReturnType.GenericTask => "Task<int>",
            ReturnType.ValueTask => "ValueTask",
            ReturnType.GenericValueTask => "ValueTask<int>",
            ReturnType.Void => "void",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public (int Line, int StartPosition, int Length) GetMethodNameSpan()
    {
        var methodHeadPrefix = $"    public {(IsAsync ? "async " : "")}{BuildReturnType()} ";
        return (6, methodHeadPrefix.Length + 1, "TestSubjectMethod".Length);
    }

    public (int Line, int StartPosition, int Length) GetTaskCallSpan()
    {
        var methodBody = $"        {BuildBody()}";
        var match = MethodCallRegex().Match(methodBody);
        return (8, match.Index + 1, match.Length);
    }

    public (int Line, int StartPosition, int Length) GetPassedTokenSpan()
    {
        var methodBody = $"        {BuildBody()}";
        var match = PassedCancellationTokenRegex().Match(methodBody);
        if (!match.Success)
        {
            throw new InvalidOperationException("No cancellation token found in the method body");
        }

        return (8, match.Index + 1, match.Length);
    }


    [GeneratedRegex(@"Task.\w+\((?:\(\))?[^)]*\)")]
    private static partial Regex MethodCallRegex();

    [GeneratedRegex(@"(?<=Task.\w+\((?:\(\))?[^)]*\, )([\w.]+)(?=\))")]
    private static partial Regex PassedCancellationTokenRegex();
}
