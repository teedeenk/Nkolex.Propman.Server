using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data
{
    public class InvoiceDataService (ILogger<InvoiceDataService> logger, IRepository<IInvoice> repo) : IInvoiceDataService<IInvoice>
    {
        public async Task<int> AddAsync(IInvoice entity)
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

        public Task<int> DeleteAsync(IInvoice entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<IInvoice>> GetAllAsync()
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

        public Task<IInvoice> GetByIdAsync(IInvoice entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(IInvoice entity)
        {
            throw new NotImplementedException();
        }
    }
}
