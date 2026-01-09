using Microsoft.AspNetCore.Http.HttpResults;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Data
{
    public class AccountDataService<Account> : IAccountDataService<IAccount>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AccountDataService<IAccount>> _logger;
        private readonly IRepository<IAccount> _repo;

        public AccountDataService(IServiceProvider serviceProvider, ILogger<AccountDataService<IAccount>> logger, IRepository<IAccount> repo)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger;
            _repo = repo;
        }
        public async Task<int> AddAsync(IAccount entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Account entity cannot be null");
            }
            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException("Account entity must have a valid ID before being saved to repository.", nameof(entity));
            }
            try
            {
                var accounts = await GetAllAsync();
                if (accounts.Any(a => a.Id == entity.Id || a.Email == entity.Email))
                {
                    throw new InvalidOperationException($"Account: {entity.Email} already exists.");
                }
                await _repo.AddAsync(entity);
                _logger.LogInformation("Account added");
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError("The following error occured: {ex}", ex.Message);
                return 0;
            }
        }

        public Task<int> DeleteAsync(IAccount entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<IAccount>> GetAllAsync()
        {
            _logger.LogInformation("Accounts fetched");
            return await _repo.GetAllAsync();
        }

        public async Task<IAccount> GetByIdAsync(IAccount account)
        {
            try
            {
                var accounts = await GetAllAsync();
                var accountById = accounts.Where(x => x.Email == account.Email).FirstOrDefault();
                if (accountById == null)
                {
                    return _serviceProvider.GetRequiredService<IAccount>();
                }
                _logger.LogInformation("account fetched");
                return accountById;
            }
            catch
            {
                return _serviceProvider.GetRequiredService<IAccount>();
            }
        }

        public Task<int> UpdateAsync(IAccount entity)
        {
            throw new NotImplementedException();
        }
    }
}
