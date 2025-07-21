using System.Threading;
using System.Threading.Tasks;

public class TestCA2016Suppression
{
    public async Task TestMethodAsync(CancellationToken cancellationToken = default)
    {
        // These cases should trigger both our CHIHC analyzer and CA2016
        // When suppression is enabled, CA2016 should be suppressed
        
        // Case 1: Missing cancellationToken parameter - should trigger CHIHC004/CHIHC005
        await Task.Delay(1000); // Missing cancellationToken
        
        // Case 2: Using CancellationToken.None - should trigger CHIHC003  
        await Task.Delay(1000, CancellationToken.None);
        
        // Case 3: Method with overload that accepts CancellationToken
        await SomeAsyncMethod(); // Our analyzer should suggest using overload with CancellationToken
    }

    private async Task SomeAsyncMethod()
    {
        await Task.Delay(500);
    }
    
    private async Task SomeAsyncMethod(CancellationToken cancellationToken)
    {
        await Task.Delay(500, cancellationToken);
    }
}