using System;
using System.Threading.Tasks;
using CanIHazCancelationTokenAnalyzer.CodeFixes;
using CanIHazCancellationTokenAnalyzer.Analyzers;
using CanIHazCancellationTokenAnalyzer.CodeFixes;
using CanIHazCancellationTokenAnalyzer.Tests.Utils;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
        CanIHazCancellationTokenAnalyzer.Analyzers.MethodInvocationAnalyzer,
        CanIHazCancellationTokenAnalyzer.CodeFixes.MethodInvocationFixProvider,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace CanIHazCancellationTokenAnalyzer.Tests.Analyzers;

public class MethodInvocationAnalyzerTests
{
    public static TheoryData<ReturnType, Method, ParameterType[]> TestData =>
        new MatrixTheoryData<ReturnType, Method, ParameterType[]>(Enum.GetValues<ReturnType>(),
            [
                Method.None, Method.IsAsync
            ],
            (ParameterType[][])[
                [ParameterType.CancellationToken], [ParameterType.CancellationTokenWithDefault],
                [ParameterType.OtherParam, ParameterType.CancellationToken],
                [ParameterType.CancellationToken, ParameterType.OtherParam],
                [ParameterType.OtherParam, ParameterType.CancellationTokenWithDefault],
                [ParameterType.OtherParamWithDefault, ParameterType.CancellationTokenWithDefault]
            ]);

    [Theory(DisplayName = "Code that passes a token should have no issues")]
    [MemberData(nameof(TestData))]
    public async Task CodeWithIssues(ReturnType returnType, Method methodConfiguration, ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, CancellationTokenUsage.PassesCancellationToken, parameterTypes);
        await VerifyCodeHasNoIssuesAsync(test);
    }

    [Theory(DisplayName = "Code that does not pass a token should ask to pass it if available")]
    [MemberData(nameof(TestData))]
    public async Task MethodsWithoutToken(ReturnType returnType,
        Method methodConfiguration,
        ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, parameterTypes);
        await VerifyAskingToAddTokenAsync(test);
        await VerifyAddedCancellationTokenAsync(test, test.WithPassesCancellationToken());
    }

    [Theory(DisplayName = "Code that passes CancellationToken.None should ask to pass a real token if available")]
    [MemberData(nameof(TestData))]
    public async Task NonePassedButRealAvailable(ReturnType returnType,
        Method methodConfiguration,
        ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType,
            methodConfiguration,
            CancellationTokenUsage.UsesNoneCancellationToken,
            parameterTypes);
        await VerifyAskingToUseRealTokenAsync(test);
        await VerifyUsedRealCancellationTokenAsync(test, test.WithPassesCancellationToken());
    }

    [Theory(DisplayName = "Code that passes a CancellationToken parameter has no issues")]
    [MemberData(nameof(TestData))]
    public async Task PassedTokens(ReturnType returnType, Method methodConfiguration, ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType,
            methodConfiguration,
            CancellationTokenUsage.PassesCancellationToken,
            parameterTypes);
        await VerifyCodeHasNoIssuesAsync(test);
    }

    private static async Task VerifyAskingToAddTokenAsync(TestCase test)
    {
        var expected = VerifyCS.Diagnostic(test.InvocationAnalyzerRuleId).WithSpan(test.GetTaskCallSpan());
        await VerifyCS.VerifyAnalyzerAsync(test.ToString(), expected);
    }

    private static async Task VerifyAskingToUseRealTokenAsync(TestCase test)
    {
        var expected = VerifyCS.Diagnostic(test.InvocationAnalyzerRuleId).WithSpan(test.GetPassedTokenSpan());
        await VerifyCS.VerifyAnalyzerAsync(test.ToString(), expected);
    }

    private static async Task VerifyCodeHasNoIssuesAsync(TestCase test)
    {
        await VerifyCS.VerifyAnalyzerAsync(test.ToString());
    }

    private static async Task VerifyAddedCancellationTokenAsync(TestCase test, TestCase fixedCode)
    {
        var expected = VerifyCS.Diagnostic(test.InvocationAnalyzerRuleId).WithSpan(test.GetTaskCallSpan());
        await VerifyCS.VerifyCodeFixAsync(test.ToString(), expected, fixedCode.ToString());
    }

    private static async Task VerifyUsedRealCancellationTokenAsync(TestCase test, TestCase fixedCode)
    {
        var expected = VerifyCS.Diagnostic(test.InvocationAnalyzerRuleId).WithSpan(test.GetPassedTokenSpan());
        await VerifyCS.VerifyCodeFixAsync(test.ToString(), expected, fixedCode.ToString());
    }
}
