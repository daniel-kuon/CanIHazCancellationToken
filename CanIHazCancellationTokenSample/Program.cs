using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// Demonstration of the code fix provider behavior with different configurations
namespace CanIHazCancellationTokenSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // This should trigger analyzer and code fix
            await SomeAsyncMethod();
        }

        // This method should trigger the analyzer for missing CancellationToken parameter
        public static async Task SomeAsyncMethod()
        {
            // This call should trigger analyzer for missing CancellationToken argument
            await Task.Delay(100);
        }

        // Example of methods that will trigger the analyzers
        public async Task MethodWithoutCancellationToken()
        {
            await Task.Delay(100);
            await AnotherAsyncMethod();
        }
        
        public async Task AnotherAsyncMethod()
        {
            await Task.Delay(200);
        }
    }
}
