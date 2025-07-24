using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data
{
    public class AccountDataService : IAccountDataService
    {
        public Task<int> AddAsync(IAccount entity)
        {
            return Task.FromResult(1);
        }
    }
}
