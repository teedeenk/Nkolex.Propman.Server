using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data
{
    public class ActivityDataService(ILogger<ActivityDataService> logger, IRepository<IActivity> repo) : IActivityDataService<IActivity>
    {
        public async Task<int> AddAsync(IActivity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            try
            {
                return await repo.AddAsync(entity);
            }
            catch (Exception ex)
            {
                logger.LogError("The following error occured: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<int> DeleteAsync(IActivity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            try
            {
                return await repo.DeleteAsync(entity);
            }
            catch (Exception ex)
            {
                logger.LogError("The following error occured: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<List<IActivity>> GetAllAsync()
        {
            try
            {
                return await repo.GetAllAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("The following error occured: {ex}", ex.Message);
                throw;
            }
        }

        public Task<IActivity> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id must be a valid, non-empty Guid.", nameof(id));
            }
            return repo.GetByIdAsync(id);
        }

        public Task<int> UpdateAsync(IActivity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return repo.UpdateAsync(entity);
        }
    }
}
