using CanIHazCancellationTokenAnalyzer.Tests.Utils;
using System;
using System.Formats.Asn1;
using System.Threading.Tasks;
using CanIHazCancellationTokenAnalyzer.Analyzers;
using Xunit;
using VerifyCS =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
        CanIHazCancellationTokenAnalyzer.Analyzers.MethodDeclarationAnalyzer,
        CanIHazCancelationTokenAnalyzer.CodeFixes.MethodDeclarationFixProvider,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace CanIHazCancellationTokenAnalyzer.Tests.Analyzers;

public class MethodDeclarationAnalyzerTests
{
    public static TheoryData<ReturnType, Method, ParameterType[]> CodeIssueData =>
        new MatrixTheoryData<ReturnType, Method, ParameterType[]>(Enum.GetValues<ReturnType>(),
            [
                Method.None, Method.IsAsync
            ],
            [[], [ParameterType.OtherParam], [ParameterType.OtherParamWithDefault]]);

    public static TheoryData<ReturnType, Method, ParameterType[]> NoIssueData =>
        new MatrixTheoryData<ReturnType, Method, ParameterType[]>(Enum.GetValues<ReturnType>(),
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

    [Theory(DisplayName = "Methods without CancellationToken parameter are detected")]
    [MemberData(nameof(CodeIssueData))]
    public async Task IssuesAreFound(ReturnType returnType, Method methodConfiguration, ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, parameterTypes);
        await VerifyAnalyzerAsync(test);
    }

    [Theory(DisplayName = "Optional CancellationToken parameter is added")]
    [MemberData(nameof(CodeIssueData))]
    public async Task IssuesAreFixed(ReturnType returnType, Method methodConfiguration, ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, parameterTypes);
        await VerifyCodeFixAsync(test, test.WithOptionalCancellationToken());
    }

    [Theory(DisplayName = "Methods with CancellationToken parameter are not marked as issues")]
    [MemberData(nameof(NoIssueData))]
    public async Task CodeWithoutIssues(ReturnType returnType,
        Method methodConfiguration,
        ParameterType[] parameterTypes)
    {
        var test = new TestCase(returnType, methodConfiguration, parameterTypes);
        await VerifyCS.VerifyAnalyzerAsync(test.ToString());
    }


    [Fact(DisplayName = "Methods that implement an interface are not marked as issues")]
    public async Task InterfaceImplementation()
    {
        //language=cs
        string code = $$"""
                        using System.Threading.Tasks;
                        using System.Threading;

                        #pragma warning disable {{Rules.TaskRule.Id}}
                        public interface ITestInterface
                        {
                            Task InterfaceMethod();
                        }
                        #pragma warning restore {{Rules.TaskRule.Id}}

                        public class TestClass : ITestInterface
                        {
                            public Task InterfaceMethod() => Task.CompletedTask;
                        }
                        """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }


    [Fact(DisplayName = "Methods that implement explicitly are not marked as issues")]
    public async Task ExplicitInterfaceImplementation()
    {
        //language=cs
        string code = $$"""
                        using System.Threading.Tasks;
                        using System.Threading;

                        #pragma warning disable {{Rules.TaskRule.Id}}
                        public interface ITestInterface
                        {
                            Task InterfaceMethod();
                        }
                        #pragma warning restore {{Rules.TaskRule.Id}}

                        public class TestClass : ITestInterface
                        {
                            Task ITestInterface.InterfaceMethod() => Task.CompletedTask;
                        }
                        """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }


    [Fact(DisplayName = "Methods that override a base class method are not marked as issues")]
    public async Task OverrideBaseClassMethods()
    {
        //language=cs
        string code = $$"""
                        using System.Threading.Tasks;
                        using System.Threading;

                        #pragma warning disable {{Rules.TaskRule.Id}}
                        public class BaseClass
                        {
                            public virtual Task TestInheritMethod() => Task.CompletedTask;
                        }
                        #pragma warning restore {{Rules.TaskRule.Id}}

                        public class TestClass : BaseClass
                        {
                            public override Task TestInheritMethod() => Task.CompletedTask;
                        }
                        """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }

    [Fact(DisplayName = "Methods that have an overloaded version with CancellationToken are not marked as issues")]
    public async Task OverloadedMethodsWithCancellationTokenAreNotMarkedAsIssues()
    {
        //language=cs
        const string code = """
        using System.Threading;
        using System.Threading.Tasks;

        public class TestClass
        {
            public Task DoWorkAsync()
            {
                return Task.CompletedTask;
            }

            public Task DoWorkAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
        """;

        await VerifyCS.VerifyAnalyzerAsync(code);
    }


    private static async Task VerifyAnalyzerAsync(TestCase test)
        {
            var expected = VerifyCS.Diagnostic(test.DeclarationAnalyzerRuleId).WithSpan(test.GetMethodNameSpan());
            await VerifyCS.VerifyAnalyzerAsync(test.ToString(), expected);
        }

        private static async Task VerifyCodeFixAsync(TestCase test, TestCase fixedCode)
        {
            var expected = VerifyCS.Diagnostic(test.DeclarationAnalyzerRuleId).WithSpan(test.GetMethodNameSpan());
            await VerifyCS.VerifyCodeFixAsync(test.ToString(), expected, fixedCode.ToString());
        }
    }
