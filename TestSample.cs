using System.Threading.Tasks;

namespace TestSample
{
    public class TestClass
    {
        // This should trigger the analyzer and offer code fix
        public async Task TestMethodAsync()
        {
            await SomeAsyncMethod();
        }
        
        public async Task SomeAsyncMethod()
        {
            await Task.Delay(1000);
        }
    }
}