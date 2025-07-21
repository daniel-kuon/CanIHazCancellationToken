using System.Threading;
using System.Threading.Tasks;

public class TestCA2016Suppression
{
    public async Task TestMethodAsync(CancellationToken cancellationToken = default)
    {
        // This should trigger both our CHIHC004/CHIHC005 and CA2016
        await Task.Delay(1000); // Missing cancellationToken
        
        // This should trigger CHIHC003 and possibly CA2016
        await Task.Delay(1000, CancellationToken.None);
    }
}