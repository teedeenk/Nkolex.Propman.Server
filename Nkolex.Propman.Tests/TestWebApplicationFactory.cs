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
                var repositoryDescriptors = services
                    .Where(d => d.ServiceType.IsGenericType &&
                                d.ServiceType.GetGenericTypeDefinition() == typeof(IRepository<>))
                    .ToList();

                foreach (var descriptor in repositoryDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<IRepository<IAccount>>(
                    new InMemoryRepository<IAccount>(a => a.Id));
                
                services.AddSingleton<IRepository<IProperty>>(
                    new InMemoryRepository<IProperty>(p => p.Id));
                
                services.AddSingleton<IRepository<IStatement>>(
                    new InMemoryRepository<IStatement>(s => s.Id));
                
                services.AddSingleton<IRepository<IInvoice>>(
                    new InMemoryRepository<IInvoice>(i => i.Id));
            });
        }
    }
}
