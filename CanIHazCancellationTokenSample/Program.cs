using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

// namespace CanIHazCancelationTokenSample
// {
//     class Program
//     {
//         static async Task Main(string[] args)
//         {
//             await Task.Delay(100, System.Threading.CancellationToken.None);
//         }
//
//         // Should trigger a warning
//         public async void AsyncVoidWithoutCancellationToken(System.Threading.CancellationToken cancellationToken =
//             default)
//         {
//             await Task.Delay(100, CancellationToken.None);
//         }
//
//         public async Task AsyncTaskWithoutCancellationToken(System.Threading.CancellationToken cancellationToken =
//             default)
//         {
//             await Task.Delay(100);
//         }
//
//         public Task NonAsyncTaskWithoutCancellationToken()
//         {
//             return Task.CompletedTask;
//         }
//
//         public async Task<int> AsyncGenericTaskWithoutCancellationToken()
//         {
//             await Task.Delay(100);
//             await Task.Delay(100);
//             return 42;
//         }
//
//         public Task<int> NonAsyncGenericTaskWithoutCancellationToken()
//         {
//             return Task.FromResult(42);
//         }
//
//         public async ValueTask<int> AsyncValueTaskWithoutCancellationToken()
//         {
//             await Task.Delay(100, System.Threading.CancellationToken.None);
//             return 42;
//         }
//
//         public ValueTask<int> NonAsyncValueTaskWithoutCancellationToken(
//             System.Threading.CancellationToken cancellationToken = default)
//         {
//             Func<Task> asyncMethod = async () => await Task.Delay(100, System.Threading.CancellationToken.None);
//
//             asyncMethod();
//
//             return new ValueTask<int>(42);
//         }
//
//         // Should trigger a warning
//         public void NonAsyncVoid()
//         {
//         }
//
//         public async Task AsyncTaskWithOptionalCancellationToken(CancellationToken cancellationToken = default)
//         {
//             await Task.Delay(100, System.Threading.CancellationToken.None);
//         }
//
//
//         public async Task AsyncTaskWithOptionalCancellationToken(int delay = 100)
//         {
//             await Task.Delay(100, System.Threading.CancellationToken.None);
//         }
//
//         public Task NonAsyncTaskWithOptionalCancellationToken(CancellationToken cancellationToken = default)
//         {
//             return AsyncTaskWithOptionalCancellationToken(100);
//         }
//
//         public async Task<int> AsyncGenericTaskWithOptionalCancellationToken(CancellationToken cancellationToken =
//             default)
//         {
//             await Task.Delay(100, System.Threading.CancellationToken.None);
//             return 42;
//         }
//
//         public Task<int> NonAsyncGenericTaskWithOptionalCancellationToken(CancellationToken cancellationToken = default)
//         {
//             return AsyncGenericTaskWithOptionalCancellationToken(cancellationToken);
//         }
//
//         public async ValueTask<int> AsyncValueTaskWithOptionalCancellationToken(CancellationToken cancellationToken =
//             default)
//         {
//             await Task.Delay(100, System.Threading.CancellationToken.None);
//             return 42;
//         }
//
//         public ValueTask<int> NonAsyncValueTaskWithOptionalCancellationToken(
//             System.Threading.CancellationToken cancellationToken = default,
//             System.Threading.CancellationToken cancellationToken2 = default)
//         {
//             return AsyncValueTaskWithOptionalCancellationToken(System.Threading.CancellationToken.None);
//         }
//
//         public ValueTask<int> TestSubjectMethod(System.Threading.CancellationToken cancellationToken, int param)
//         {
//             return new ValueTask<int>(Task.Run(() => 42, CancellationToken.None));
//         }
//     }
// }


public class MethodsWithoutCancellationTokens
{

    class Inspections
    {
        public async Task<int> GenericTask()
        {
            await Task.Delay(100);
            return 42;
        }

        public async Task NonGenericTask()
        {
            await Task.Delay(100);
        }

        public async void AsyncVoidMethod()
        {
            await Task.Delay(100);
        }

        public ValueTask<int> NonGenericValueTask()
        {
            return new ValueTask<int>(Task.Run(() => 42));
        }

        public ValueTask<int> GenericValueTask()
        {
            return new ValueTask<int>(Task.Run(() => 42));
        }

    }
    class Fixed
    {
        public async Task<int> GenericTask(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(100);
            return 42;
        }

        public async Task NonGenericTask(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(100);
        }

        public async void AsyncVoidMethod(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(100);
        }

        public ValueTask<int> NonGenericValueTask(System.Threading.CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(Task.Run(() => 42));
        }

        public ValueTask<int> GenericValueTask(System.Threading.CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(Task.Run(() => 42));
        }
    }
}
