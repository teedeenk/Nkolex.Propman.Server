using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data.Repositories
{
    public class FlatFileRepository : IRepository<IAccount>
    {
        public Task<int> AddAsync(IAccount entity)
        {
            throw new NotImplementedException();
        }
    }
}
