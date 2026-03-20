using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Tests
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IRepository<>));
                
                services.AddSingleton<IRepository<IAccount>>(sp => 
                    new InMemoryRepository<IAccount>(a => a.Id));
                
                services.AddSingleton<IRepository<IProperty>>(sp => 
                    new InMemoryRepository<IProperty>(p => p.Id));
                
                services.AddSingleton<IRepository<IStatement>>(sp => 
                    new InMemoryRepository<IStatement>(s => s.Id));
            });
        }
    }
}
