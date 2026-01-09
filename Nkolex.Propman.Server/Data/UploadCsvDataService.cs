using Microsoft.Extensions.Caching.Memory;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Services;

namespace Nkolex.Propman.Server.Data
{
    public class UploadCsvDataService : IUploadCsvDataService<Statement, StatementLine>
    {
        private readonly ILogger<UploadCsvDataService> _logger;
        private readonly IRepository<IStatement> _repo;

        public UploadCsvDataService(ILogger<UploadCsvDataService> logger, IRepository<IStatement> repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<int> AddAsync(Statement statement)
        {
            ArgumentNullException.ThrowIfNull(statement);
            try
            {
                var repo = await _repo.AddAsync(statement);
                return repo;
            }
            catch (Exception ex)
            {
                _logger.LogError("The following error occured: {ex}", ex.Message);
                throw;
            }
        }

        public async Task<List<Statement>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Get all statement... | GetAllAsync uploadDatatCsvService");
                var repo = await _repo.GetAllAsync();
                var converted = ConvertInterfaceToConcrete(repo);
                _logger.LogInformation("Returning statement {statement} | GetAllAsync uploadDataCsvService", converted);
                return converted;
            }
            catch (Exception ex)
            {
                _logger.LogError("{ex.Message}", ex);
                throw;
            }
        }

        public Task<int> UpdateAsync(Statement statement)
        {
            ArgumentNullException.ThrowIfNull(statement);
            return Task.FromResult(1);
        }

        public Task<int> DeleteAsync(Statement statement)
        {
            throw new NotImplementedException();
        }

        private static List<Statement> ConvertInterfaceToConcrete(List<IStatement> statements)
        {
            var converted = new List<Statement>();
            foreach (var s in statements)
            {
                if (s is Statement statement)
                {
                    converted.Add(statement);
                }
            }
            return converted;
        }
    }
}
