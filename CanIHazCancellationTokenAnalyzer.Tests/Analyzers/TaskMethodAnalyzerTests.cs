using CanIHazCancellationTokenAnalyzer.Tests.Utils;
using System;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    CanIHazCancellationTokenAnalyzer.Analyzers.TaskMethodAnalyzer,
    CanIHazCancelationTokenAnalyzer.CodeFixes.TaskMethodFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace CanIHazCancellationTokenAnalyzer.Tests.Analyzers;

public class TaskMethodAnalyzerTests
{
    public static TheoryData<ReturnType, Method, ParameterType[]> CodeIssueData =>
        new MatrixTheoryData<ReturnType, Method, ParameterType[]>(
            Enum.GetValues<ReturnType>(),
            [
                Method.None, Method.IsAsync
            ], [[], [ParameterType.OtherParam], [ParameterType.OtherParamWithDefault]]);

    public static TheoryData<ReturnType, Method, ParameterType[]> NoIssueData =>
        new MatrixTheoryData<ReturnType, Method, ParameterType[]>(
            Enum.GetValues<ReturnType>(),
            [
                Method.None, Method.IsAsync
            ],
            [
                [ParameterType.CancellationToken], [ParameterType.CancellationTokenWithDefault],
                [ParameterType.OtherParam, ParameterType.CancellationToken],
                [ParameterType.CancellationToken, ParameterType.OtherParam],
                [ParameterType.OtherParam, ParameterType.CancellationTokenWithDefault],
                [ParameterType.OtherParamWithDefault, ParameterType.CancellationTokenWithDefault]
            ]);

    [Theory(DisplayName = "Issues are found")]
    [MemberData(nameof(CodeIssueData))]
    public async Task IssuesAreFound(ReturnType returnType, Method methodConfiguration,
        ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, parameterTypes);
        await VerifyAnalyzerAsync(test);
    }

    [Theory(DisplayName = "Issues are fixed")]
    [MemberData(nameof(CodeIssueData))]
    public async Task IssuesAreFixed(ReturnType returnType, Method methodConfiguration,
        ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, parameterTypes);
        await VerifyCodeFixAsync(test, test.WithOptionalCancellationToken());
    }

    [Theory(DisplayName = "Code without issues")]
    [MemberData(nameof(NoIssueData))]
    public async Task CodeWithoutIssues(ReturnType returnType, Method methodConfiguration,
        ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, parameterTypes);
        await VerifyCS.VerifyAnalyzerAsync(test.ToString());
    }

    private async Task VerifyAnalyzerAsync(TestCase test)
    {
        var expected = VerifyCS.Diagnostic("CHIHC001").WithSpan(test.GetMethodNameSpan());
        await VerifyCS.VerifyAnalyzerAsync(test.ToString(), expected);
    }

    private async Task VerifyCodeFixAsync(TestCase test, TestCase fixedCode)
    {
        var expected = VerifyCS.Diagnostic("CHIHC001").WithSpan(test.GetMethodNameSpan());
        await VerifyCS.VerifyCodeFixAsync(test.ToString(), expected, fixedCode.ToString());
    }
}
