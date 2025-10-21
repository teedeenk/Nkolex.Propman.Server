using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data
{
    public class AccountDataService : IAccountDataService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AccountDataService> _logger;
        private readonly IRepository<IAccount> _repo;

        public AccountDataService(IServiceProvider serviceProvider, ILogger<AccountDataService> logger, IRepository<IAccount> repo)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger;
            _repo = repo;
        }
        public Task<int> AddAsync(IAccount entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Account entity cannot be null");
            }
            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException("Account entity must have a valid ID before being saved to repository.", nameof(entity));
            }
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    var repository = scope.ServiceProvider.GetRequiredService<IRepository<IAccount>>();
            //    repository.AddAsync(entity);
            //}
            _repo.AddAsync(entity);
            _logger.LogInformation("Account added");
            return Task.FromResult(1);
        }

        public Task<int> DeleteAsync(IAccount entity)
        {
            throw new NotImplementedException();
        }

        public Task<List<IAccount>> GetAllAsync()
        {
            _logger.LogInformation("Accounts fetched");
            return _repo.GetAllAsync();
        }

        public Task<IAccount> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(IAccount entity)
        {
            throw new NotImplementedException();
        }
    }
}
