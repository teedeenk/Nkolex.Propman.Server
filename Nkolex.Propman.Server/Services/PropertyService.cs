using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyDataService<IProperty> _dataService;
        private readonly IAccountDataService<IAccount> _accountDataService;

        public PropertyService(IPropertyDataService<IProperty> dataService, IAccountDataService<IAccount> accountDataService)
        {
            _dataService = dataService;
            _accountDataService = accountDataService;
        }

        public async Task<List<PropertyResponse>> GetAllPropertiesAsync()
        {
            try
            {
                var properties = await _dataService.GetAllAsync();
                var accounts = await _accountDataService.GetAllAsync();
                return properties.Select(property => MapToResponse(property, accounts)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving properties: {ex.Message}", ex);
            }
        }

        public async Task<PropertyResponse> GetByIdAsync(IProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            var result = await _dataService.GetByIdAsync(property);
            var accounts = await _accountDataService.GetAllAsync();

            return MapToResponse(result, accounts);
        }

        public async Task<PropertyResponse> UpdatePropertyAsync(IProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            await _dataService.UpdateAsync(property);
            var accounts = await _accountDataService.GetAllAsync();

            return MapToResponse(property, accounts);
        }

        public async Task<PropertyResponse> UploadPropertyAsync(IProperty property)
        {
            await _dataService.AddAsync(property);
            var accounts = await _accountDataService.GetAllAsync();

            return MapToResponse(property, accounts);
        }

        private static PropertyResponse MapToResponse(IProperty property, List<IAccount> accounts)
        {
            var manager = accounts.FirstOrDefault(a => a.Id == property.PropertyManager);
            var tenants = accounts.Where(a => property.Tenants.Contains(a.Id));

            return new PropertyResponse
            {
                Id = property.Id,
                Name = property.Name,
                Address = property.Address,
                PropertyType = property.PropertyType,
                Statement = property.Statement,
                PropertyManager = manager is not null ? ToAccountSummary(manager) : null,
                Tenants = [.. tenants.Select(ToAccountSummary)]
            };
        }

        private static AccountSummary ToAccountSummary(IAccount account)
        {
            return new AccountSummary
            {
                Id = account.Id,
                Name = account.Name,
                Surname = account.Surname,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber
            };
        }
    }
}

