using System.Threading.Tasks;

// Example showing analyzer behavior with different configurations
namespace ConfigurationExample
{
    public class AsyncService
    {
        // These methods will trigger the analyzer for missing CancellationToken parameters
        public async Task ProcessDataAsync()
        {
            await Task.Delay(100);
        }

        public async Task<string> GetDataAsync()
        {
            await ProcessDataAsync();
            return "data";
        }

        // These method calls will trigger analyzer for missing CancellationToken arguments
        public async Task TestMethodAsync()
        {
            await Task.Delay(500); // Should suggest adding CancellationToken.None or parameter
            await ProcessDataAsync(); // Should suggest passing CancellationToken
        }
    }

    // Expected output with default configuration (CanIHazCancellationToken.PreferUsingStatements = false):
    // public async Task ProcessDataAsync(System.Threading.CancellationToken cancellationToken = default)
    // await Task.Delay(500, System.Threading.CancellationToken.None);

    // Expected output with new configuration (CanIHazCancellationToken.PreferUsingStatements = true):
    // using System.Threading;
    // public async Task ProcessDataAsync(CancellationToken cancellationToken = default)
    // await Task.Delay(500, CancellationToken.None);
}