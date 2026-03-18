using Microsoft.AspNetCore.Http.HttpResults;
using Nkolex.Propman.Server.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Nkolex.Propman.Server.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IServiceProvider _serviceProvider;
        public PropertyService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IProperty> GetByIdAsync(IProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            using var scope = _serviceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IPropertyDataService<IProperty>>();
            await dataService.GetByIdAsync(property);

            return property;
        }

        public async Task<IProperty> UpdatePropertyAsync(IProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            using var scope = _serviceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IPropertyDataService<IProperty>>();
            await dataService.UpdateAsync(property);

            return property;
        }

        public async Task<IProperty> UploadPropertyAsync(IProperty property)
        {
            using var scope =  _serviceProvider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IPropertyDataService<IProperty>>();
            await dataService.AddAsync(property);

            return property;
        }
    }
}
