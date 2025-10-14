using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data
{
    public class AccountDataService : IAccountDataService
    {
        private readonly IServiceProvider _serviceProvider;

        public AccountDataService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository<IAccount>>();
                repository.AddAsync(entity);
            }
            return Task.FromResult(1);
        }
    }
}
