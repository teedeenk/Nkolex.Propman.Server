using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;

namespace Nkolex.Propman.Tests
{
    public class TestFixture : IDisposable
    {
        public WebApplicationFactory<Program> Factory { get; private set; }
        public IServiceScope TestScope { get; private set; }
        public IServiceProvider ServiceProvider => TestScope.ServiceProvider;

        public TestFixture(WebApplicationFactory<Program> factory)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            TestScope = Factory.Services.CreateScope();
        }

        public void Dispose()
        {
            TestScope.Dispose();
            Factory.Dispose();
        }
    }
}