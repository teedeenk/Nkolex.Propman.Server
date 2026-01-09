using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Services;
using NSubstitute;
using System.Reflection.PortableExecutable;

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

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IAccountService, AccountService>();
            serviceCollection.AddTransient<ICreateAccountRequest, CreateAccountRequest>();
            serviceCollection.AddTransient<ICreateAccountResponse, CreateAccountResponse>();
            var mockAccountRepository = Substitute.For<IRepository<IAccount>>();
            serviceCollection.AddScoped(_ => mockAccountRepository);
            serviceCollection.AddTransient<IAccountDataService<IAccount>, AccountDataService<IAccount>>();
            serviceCollection.AddTransient<IAuthService, AuthService>();
            serviceCollection.AddTransient<IUploadCsvDataService<Statement, StatementLine>, UploadCsvDataService>();
            serviceCollection.AddScoped<IProcessCsvFileService,  ProcessCsvFileService>();
            serviceCollection.AddTransient<IStatement, Statement>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            TestScope = serviceProvider.CreateScope();
        }
        public void Dispose()
        {
            TestScope.Dispose();
            Factory.Dispose();
        }
    }
}