using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Services;

namespace Nkolex.Propman.Tests
{
    public class TestFixture : IDisposable
    {
        public WebApplicationFactory<Program> _factory { get; private set; }
        public IServiceScope TestScope { get; private set; }
        public IServiceProvider ServiceProvider => TestScope.ServiceProvider;

        public TestFixture(WebApplicationFactory<Program> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            TestScope = _factory.Services.CreateScope();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IAccountService, AccountService>();
            serviceCollection.AddTransient<ICreateAccountRequest, CreateAccountRequest>();
            serviceCollection.AddTransient<ICreateAccountResponse, CreateAccountResponse>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            TestScope = serviceProvider.CreateScope();
        }
        public void Dispose()
        {
            TestScope.Dispose();
            _factory.Dispose();
        }
    }
}
