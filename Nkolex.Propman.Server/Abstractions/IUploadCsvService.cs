using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IUploadCsvService
    {
        Task<int> AddAsync(IStatement statement);
        Task<List<Statement>> GetAllAsync();
    }
}
