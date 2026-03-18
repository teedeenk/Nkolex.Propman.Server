using Nkolex.Propman.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Nkolex.Propman.Server.Data
{
    public class PropertyDataService<TProperty> : IPropertyDataService<TProperty> where TProperty : IProperty
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PropertyDataService<TProperty>> _logger;
        private readonly IRepository<TProperty> _repo;

        public PropertyDataService(IServiceProvider serviceProvider, ILogger<PropertyDataService<TProperty>> logger, IRepository<TProperty> repo)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger;
            _repo = repo;
        }

        public async Task<int> AddAsync(TProperty entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Property entity cannot be null");
            }
            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException("Property entity must have a valid ID before being saved to repository.", nameof(entity));
            }
            try
            {
                var properties = await GetAllAsync();
                if (properties.Any(p => p.Id == entity.Id))
                {
                    throw new InvalidOperationException($"Property with ID: {entity.Id} already exists.");
                }
                await _repo.AddAsync(entity);
                _logger.LogInformation("Property added");
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("The following error occured: {ex}", ex.Message);
                return 0;
            }
        }

        public Task<int> DeleteAsync(TProperty entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TProperty>> GetAllAsync()
        {
            _logger.LogInformation("Properties fetched");
            return await _repo.GetAllAsync();
        }

        public Task<TProperty> GetByIdAsync(TProperty entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return _repo.GetByIdAsync(entity);
        }

        public Task<int> UpdateAsync(TProperty entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return _repo.UpdateAsync(entity);
        }
    }
}
